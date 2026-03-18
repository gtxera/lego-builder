using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public class Piece : MonoBehaviour
{
    private PieceColor[] _colors;
    
    private Rigidbody _rigidbody;

    private readonly Dictionary<int, Action<Color, bool>> _onColorChangedCallbacks = new();

    private GameObject _connectorsRoot;
    private readonly List<Socket> _sockets = new();
    private readonly List<Stud> _studs = new();
    private readonly List<AnchorPoint> _anchors = new();
    private readonly List<PieceConnector> _connectors = new();

    private readonly Collider[] _overlaps = new Collider[32];

    private PieceRotation _rotation;

    private IEnumerable<PieceColoredPart> _coloredParts;

    private float _creationTime;
    private Vector3 _worldSize;
    private Vector3 _baseHalfSize;
    private Vector3 _rotatedHalfSize;
    private Vector3 _rotatedSize;
    private bool _connectionsSuspended;

    [SerializeField]
    private float _lastMovementTime;

    private static int NonConnectorLayerMask;
    private static int AnchorLayerMask;

    public IPieceTemplate Template { get; private set; }

    public Guid Id { get; private set; }

    public IReadOnlyList<PieceColor> Colors => _colors;

    public IEnumerable<Piece> ConnectedPieces
    {
        get
        {
            var uniquePieces = new HashSet<Piece>();
            foreach (var connector in _connectors)
            {
                var connectedPiece = connector.ConnectedPiece;
                if (connectedPiece != null && uniquePieces.Add(connectedPiece))
                    yield return connectedPiece;
            }
        }
    }
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

        if (NonConnectorLayerMask == 0)
            NonConnectorLayerMask = ~LayerMask.GetMask("Connectors", "Anchors");

        if (AnchorLayerMask == 0)
            LayerMask.GetMask("Anchors");
    }

    public void Initialize(IPieceTemplate template)
    {
        Template = template;
        Template.Configure(gameObject);

        _coloredParts = GetComponentsInChildren<PieceColoredPart>();
        
        _colors = new PieceColor[Template.GetColorCount()];
        _worldSize = Template.GetSize().ToWorld();
        _baseHalfSize = _worldSize / 2f;
        RefreshRotationCache();

        Id = Guid.NewGuid();

        _connectorsRoot = new GameObject("Connectors");
        var rootTransform = _connectorsRoot.transform;
        rootTransform.SetParent(transform);
        
        foreach (var socketPosition in template.GetSocketPositions())
        {
            CreateConnector("Socket", rootTransform, socketPosition, _sockets);
        }

        foreach (var studPosition in template.GetStudPositions())
        {
            CreateConnector("Stud", rootTransform, studPosition, _studs);
        }

        var size = Template.GetSize();
        var halfSize = size.ToWorld() / 2f;

        for (var x = 0; x < size.X; x++)
        {
            var backPosition = new Vector3(Conversions.ToWorld(x) + Conversions.PieceToWorld / 2f - halfSize.x, -halfSize.y, -halfSize.z);
            var frontPosition = new Vector3(Conversions.ToWorld(x) + Conversions.PieceToWorld / 2f - halfSize.x, -halfSize.y, halfSize.z);
            
            CreateConnector("Back Anchor", rootTransform, backPosition, _anchors, Quaternion.LookRotation(Vector3.back));
            CreateConnector("Front Anchor", rootTransform, frontPosition, _anchors, Quaternion.LookRotation(Vector3.forward));
        }
        
        for (var y = 0; y < size.Y; y++)
        {
            var rightPosition = new Vector3(halfSize.x, -halfSize.y, Conversions.ToWorld(y) + Conversions.PieceToWorld / 2f - halfSize.z);
            var leftPosition = new Vector3(-halfSize.x, -halfSize.y, Conversions.ToWorld(y) + Conversions.PieceToWorld / 2f - halfSize.z);

            CreateConnector("Right Anchor", rootTransform, rightPosition, _anchors, Quaternion.LookRotation(Vector3.right));
            CreateConnector("Left Anchor", rootTransform, leftPosition, _anchors, Quaternion.LookRotation(Vector3.left));
        }

        CreateConnectorCache();
        _creationTime = Time.time;
    }

    private void CreateConnector<TConnector>(
        string objectName,
        Transform parent,
        Vector3 position,
        List<TConnector> connectors,
        Quaternion rotation = default) where TConnector : PieceConnector
    {
        if (rotation == default)
            rotation = Quaternion.identity;

        var connectorObject = new GameObject(objectName);
        var connectorTransform = connectorObject.transform;
        connectorTransform.SetParent(parent);
        connectorTransform.localPosition = position;
        connectorTransform.localRotation = rotation;
        
        var connector = connectorObject.AddComponent<TConnector>();
        connector.Initialize(this);
        connectors.Add(connector);
    }
    
    public void Initialize(PieceData pieceData)
    {
        Initialize(pieceData.Template);

        var transientData = pieceData.TransientData;

        if (transientData.Id != default)
            Id = transientData.Id;
        
        SetRotation(transientData.Rotation);
        MoveTo(transientData.WorldPosition);

        for (int i = 0; i < transientData.Colors.Length; i++)
            TrySetColor(transientData.Colors[i], i);
        

        _creationTime = transientData.CreationTime;
    }
    
    public Vector3 MoveTo(Vector3 position)
    {
        if (!_connectionsSuspended)
            DisconnectAllConnectors();
        
        var gridPosition = GetGridPosition(position);
        _rigidbody.position = gridPosition;
        _rigidbody.PublishTransform();
        
        _lastMovementTime = Time.time;
        
        if (!_connectionsSuspended)
            ReconnectAllConnectors();

        return gridPosition;
    }

    public void BeginDragging()
    {
        if (_connectionsSuspended)
            return;

        _connectionsSuspended = true;
        DisconnectAllConnectors();
    }

    public void EndDragging()
    {
        if (!_connectionsSuspended)
            return;

        _connectionsSuspended = false;
        ReconnectAllConnectors();
    }

    public bool TryGetAnchoredPosition(Ray ray, out Vector3 anchoredPosition)
    {
        var originalPosition = _rigidbody.position;
        _rigidbody.position = new Vector3(1000, 1000, 1000);

        if (!Physics.Raycast(ray, out var hit, float.MaxValue, NonConnectorLayerMask))
        {
            _rigidbody.position = originalPosition;
            anchoredPosition = Vector3.zero;
            return false;
        }

        var position = hit.point;
        var normal = hit.normal;
        var halfSize = _rotatedHalfSize;
        var pushHalfSize = _rotatedHalfSize;
        
        var centerPosition = GetGridPosition(position + GetPushOutFromNormal(normal, pushHalfSize));
        
        halfSize -= new Vector3(0.002f, 0.002f, 0.002f);
        
        var hits = Physics.OverlapBoxNonAlloc(centerPosition, halfSize, _overlaps, _rigidbody.rotation,
            NonConnectorLayerMask);
        if (hits == 0)
        {
            _rigidbody.position = originalPosition;
            anchoredPosition = centerPosition;
            return true;
        }
        
        var bottomPosition = centerPosition;
        bottomPosition.y -= halfSize.y;

        //halfSize += new Vector3(0.055f, 0.055f, 0.055f);
        hits = Physics.OverlapBoxNonAlloc(bottomPosition, halfSize, _overlaps, _rigidbody.rotation, AnchorLayerMask);
        //halfSize -= new Vector3(0.055f, 0.055f, 0.055f);

        AnchorPoint closestAnchor = null;
        var closestDistance = float.MaxValue;
        for (var i = 0; i < hits; i++)
        {
            var anchor = _overlaps[i].GetComponent<AnchorPoint>();
            if (anchor.Connected && !anchor.IsConnectedTo(this))
                continue;

            var distance = Vector3.Distance(anchor.transform.position, position);
            if (distance < closestDistance)
            {
                closestAnchor = anchor;
                closestDistance = distance;
            }
        }

        if (closestAnchor == null)
        {
            _rigidbody.position = originalPosition;
            anchoredPosition = Vector3.zero;
            return false;
        }

        closestDistance = float.MaxValue;
        var bestPosition = Vector3.zero;
        var foundNoCollisions = false;
        foreach (var anchor in _anchors)
        {
            if (!anchor.IsCompatible(closestAnchor))
                continue;

            var anchorRelativeCenter = GetGridPosition(closestAnchor.transform.position - anchor.GetDistanceToCenter().Rotated(_rigidbody.rotation));
            hits = Physics.OverlapBoxNonAlloc(anchorRelativeCenter, halfSize, _overlaps, _rigidbody.rotation,
                NonConnectorLayerMask);

            var distance = Vector3.Distance(anchorRelativeCenter, position);
            
            if (hits == 0 && distance < closestDistance)
            {
                foundNoCollisions = true;
                closestDistance = distance;
                bestPosition = anchorRelativeCenter;
            }
        }

        if (foundNoCollisions)
        {
            _rigidbody.position = originalPosition;
            anchoredPosition = bestPosition;
            return true;
        }

        _rigidbody.position = originalPosition;
        anchoredPosition = Vector3.zero;
        return false;
    }

    private Vector3 GetPushOutFromNormal(Vector3 normal, Vector3 size)
    {
        return Vector3.Scale(size, normal);
    }

    public Vector3 GetSweepPosition(Vector3 origin, Vector3 direction)
    {
        var originalPosition = _rigidbody.position;
        direction.Normalize();
        
        _connectorsRoot.SetActive(false);
        
        _rigidbody.position = origin;

        if (!_rigidbody.SweepTest(direction, out var hit, Mathf.Infinity, QueryTriggerInteraction.Ignore))
        {
            _rigidbody.position = originalPosition;
            _connectorsRoot.SetActive(true);
            return Vector3.zero;
        }

        var originalPoint = hit.point - direction * hit.distance;
        var center = origin - originalPoint;
        var position = GetGridPosition(hit.point + center);

        _rigidbody.position = originalPosition;
        
        _connectorsRoot.SetActive(true);
        
        return position;
    }

    private Vector3 GetGridPosition(Vector3 position)
    {
        var cornerPosition = position - _rigidbody.rotation * _baseHalfSize;
        var gridSnappedPosition = PieceVector.FromWorld(cornerPosition).ToWorld();

        return gridSnappedPosition + _rigidbody.rotation * _baseHalfSize;
    }

    private void SetRotation(PieceRotation rotation)
    {
        _rotation = rotation;
        var quaternion = Quaternion.AngleAxis(_rotation.ToAngle(), Vector3.up);
        _rigidbody.rotation = Quaternion.Inverse(transform.parent.rotation) * quaternion;
        RefreshRotationCache();
        _rigidbody.PublishTransform();
    }

    public void SetWorldRotation(float angle)
    {
        var quaternion = Quaternion.AngleAxis(angle, Vector3.up);
        var localRotation = Quaternion.Inverse(transform.localRotation) * quaternion;
        _rotation = PieceRotationExtensions.FromAngle(localRotation.eulerAngles.y);
        _rigidbody.rotation = quaternion;
        RefreshRotationCache();
        _rigidbody.PublishTransform();
    }

    public void RotateClockwise()
    {
        var rotation = Quaternion.AngleAxis(90f, Vector3.up);
        _rotation = PieceRotationExtensions.Add(_rotation, PieceRotation.East);
        _rigidbody.rotation *= rotation;
        RefreshRotationCache();
        MoveTo(_rigidbody.position);
    }

    public bool TrySetColor(PieceColor color, int index)
    {
        if (index >= _colors.Length)
            return false;
        
        if (_colors[index] is SwatchColor oldSwatchColor)
        {
            if (_onColorChangedCallbacks.TryGetValue(index, out var callback))
                oldSwatchColor.ColorChanged -= callback;
        }
        
        _colors[index] = color;

        if (color is SwatchColor swatchColor)
        {
            Action<Color, bool> callback = (callbackColor, tranparent) => OnColorChanged(callbackColor, tranparent, index);
            _onColorChangedCallbacks[index] = callback;
            swatchColor.ColorChanged += callback;
        }
        
        OnColorChanged(color.Color, color.Transparent, index);

        return true;
    }

    private void OnColorChanged(Color color, bool transparent, int index)
    {
        foreach (var coloredPart in _coloredParts)
            coloredPart.SetColor(color, transparent);
    }

    public bool MovedMoreRecentlyThan(Piece piece)
    {
        return _lastMovementTime > piece._lastMovementTime;
    }

    public PieceTransientData GetTransientData() => new(Id, transform.localPosition, _colors.ToArray(), _rotation, _creationTime, _rigidbody.position);

    public PieceData GetData() => new(Template, GetTransientData());

    public override int GetHashCode() => Id.GetHashCode();

    public Bounds GetBounds()
    {
        return new Bounds(Vector3.zero, _rotatedSize);
    }

    private void OnDestroy()
    {
        Template.OnDestroy(gameObject);
        
        for (int i = 0; i < _colors.Length; i++)
        {
            if (_colors[i] is SwatchColor swatchColor)
                if (_onColorChangedCallbacks.TryGetValue(i, out var callback))
                    swatchColor.ColorChanged -= callback;
        }
        
        foreach (var socket in _sockets)
            socket.Disconnect();
        foreach (var stud in _studs)
            stud.Disconnect();
        foreach (var anchor in _anchors)
            anchor.Disconnect();
    }

    private void RefreshRotationCache()
    {
        _rotatedHalfSize = _baseHalfSize;
        _rotatedSize = _worldSize;

        if (_rotation is PieceRotation.East or PieceRotation.West)
        {
            (_rotatedHalfSize.x, _rotatedHalfSize.z) = (_rotatedHalfSize.z, _rotatedHalfSize.x);
            (_rotatedSize.x, _rotatedSize.z) = (_rotatedSize.z, _rotatedSize.x);
        }
    }

    private void DisconnectAllConnectors()
    {
        foreach (var connector in _connectors)
            connector.Disconnect();
    }

    private void ReconnectAllConnectors()
    {
        foreach (var connector in _connectors)
            connector.Connect();
    }

    private void CreateConnectorCache()
    {
        _connectors.Clear();

        foreach (var anchor in _anchors)
            _connectors.Add(anchor);

        foreach (var socket in _sockets)
            _connectors.Add(socket);

        foreach (var stud in _studs)
            _connectors.Add(stud);
    }
}
