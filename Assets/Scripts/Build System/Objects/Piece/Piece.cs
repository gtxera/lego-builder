using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class Piece : MonoBehaviour
{
    private PieceColor[] _colors;
    
    private Rigidbody _rigidbody;

    private readonly Dictionary<int, Action<Color>> _onColorChangedCallbacks = new();

    private GameObject _connectorsRoot;
    private readonly List<Socket> _sockets = new();
    private readonly List<Stud> _studs = new();
    private readonly List<AnchorPoint> _anchors = new();

    private readonly Collider[] _overlaps = new Collider[32];

    private PieceRotation _rotation;

    private IEnumerable<Renderer> _renderers;
    
    [SerializeField]
    private float _lastMovementTime;

    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

    public IPieceTemplate Template { get; private set; }

    public Guid Id { get; private set; }

    public IReadOnlyList<PieceColor> Colors => _colors;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
    }

    public void Initialize(IPieceTemplate template)
    {
        Template = template;
        Template.Configure(gameObject);

        _renderers = GetComponentsInChildren<Renderer>();
        
        _colors = new PieceColor[Template.GetColorCount()];

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
        connectorTransform.rotation = rotation;
        
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
        
        MoveTo(transientData.Position);

        for (int i = 0; i < transientData.Colors.Length; i++)
            TrySetColor(transientData.Colors[i], i);
        
        SetRotation(transientData.Rotation);
    }

    public void MoveDifference(Vector3 difference)
    {
        _rigidbody.position += difference;
        _rigidbody.PublishTransform();
    }
    
    public Vector3 MoveTo(Vector3 position)
    {
        foreach (var socket in _sockets)
            socket.Disconnect();
        foreach (var stud in _studs)
            stud.Disconnect();
        foreach (var anchor in _anchors)
            anchor.Disconnect();
        
        var gridPosition = GetGridPosition(position);
        _rigidbody.position = gridPosition;
        _rigidbody.PublishTransform();
        
        _lastMovementTime = Time.time;
        
        foreach (var socket in _sockets)
            socket.Connect();
        foreach (var stud in _studs)
            stud.Connect();
        foreach (var anchor in _anchors)
            anchor.Connect();

        return gridPosition;
    }

    public bool TryGetAnchoredPosition(Ray ray, out Vector3 anchoredPosition)
    {
        var originalPosition = _rigidbody.position;
        _rigidbody.position = new Vector3(1000, 1000, 1000);

        if (!Physics.Raycast(ray, out var hit, float.MaxValue, ~LayerMask.GetMask("Anchors", "Connectors")))
        {
            _rigidbody.position = originalPosition;
            anchoredPosition = Vector3.zero;
            return false;
        }

        var position = hit.point;
        var normal = hit.normal;
        
        var size = Template.GetSize().ToWorld();

        var halfSize = size / 2f;
        
        var centerPosition = GetGridPosition(position + GetPushOutFromNormal(normal, halfSize));

        halfSize -= new Vector3(0.005f, 0.005f, 0.005f);
        
        var hits = Physics.OverlapBoxNonAlloc(centerPosition, halfSize, _overlaps, transform.rotation,
            ~LayerMask.GetMask("Connectors", "Anchors"));
        if (hits == 0)
        {
            _rigidbody.position = originalPosition;
            anchoredPosition = centerPosition;
            return true;
        }

        halfSize += new Vector3(0.1f, 0.1f, 0.1f);

        hits = Physics.OverlapBoxNonAlloc(centerPosition, halfSize, _overlaps, transform.rotation,
            LayerMask.GetMask("Anchors"));
        AnchorPoint closestAnchor = null;
        var closestDistance = float.MaxValue;
        for (var i = 0; i < hits; i++)
        {
            var anchor = _overlaps[i].GetComponent<AnchorPoint>();

            var distance = (anchor.transform.position - position).magnitude;
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
        
        halfSize -= new Vector3(0.11f, 0.11f, 0.11f);
        return TryConnectToAnchor(closestAnchor, out anchoredPosition);

        bool TryConnectToAnchor(AnchorPoint targetAnchor, out Vector3 anchoredPosition)
        {
            var anchors = _anchors
                .Where(anchor => anchor.IsCompatible(targetAnchor));
        
            foreach (var anchor in anchors)
            {
                var anchorRelativeCenter = GetGridPosition(targetAnchor.transform.position - anchor.GetDistanceToCenter());
                hits = Physics.OverlapBoxNonAlloc(anchorRelativeCenter, halfSize, _overlaps, transform.rotation,
                    ~LayerMask.GetMask("Anchors", "Connectors"));
                if (hits == 0)
                {
                    _rigidbody.position = originalPosition;
                    anchoredPosition = anchorRelativeCenter;
                    return true;
                }
            }

            _rigidbody.position = originalPosition;
            anchoredPosition = Vector3.zero;
            return false;
        }
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
            return Vector3.zero;

        var originalPoint = hit.point - direction * hit.distance;
        var center = origin - originalPoint;
        var position = GetGridPosition(hit.point + center);

        _rigidbody.position = originalPosition;
        
        _connectorsRoot.SetActive(true);
        
        return position;
    }

    private Vector3 GetGridPosition(Vector3 position)
    {
        var halfSize = Template.GetSize().ToWorld() / 2;
        var cornerPosition = position - _rigidbody.rotation * halfSize;
        var gridSnappedPosition = PieceVector.FromWorld(cornerPosition).ToWorld();

        return gridSnappedPosition + _rigidbody.rotation * halfSize;
    }

    public void SetRotation(PieceRotation rotation)
    {
        _rotation = rotation;
        var quaternion = Quaternion.AngleAxis(_rotation.ToAngle(), Vector3.up);
        _rigidbody.rotation = quaternion;
    }

    public void RotateClockwise()
    {
        var rotation = Quaternion.AngleAxis(90f, Vector3.up);
        _rotation = PieceRotationExtensions.Add(_rotation, PieceRotation.East);
        _rigidbody.rotation *= rotation;
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
            Action<Color> callback = callbackColor => OnColorChanged(callbackColor, index);
            _onColorChangedCallbacks[index] = callback;
            swatchColor.ColorChanged += callback;
        }
        
        OnColorChanged(color.Color, index);

        return true;
    }

    private void OnColorChanged(Color color, int index)
    {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor(BaseColorPropertyId, color);

        foreach (var renderer in _renderers)
            renderer.SetPropertyBlock(propertyBlock);
    }

    public bool MovedMoreRecentlyThan(Piece piece)
    {
        return _lastMovementTime > piece._lastMovementTime;
    }

    public PieceTransientData GetTransientData() => new(Id, transform.localPosition, _colors, _rotation);

    public PieceData GetData() => new(Template, GetTransientData());

    public override int GetHashCode() => Id.GetHashCode();

    public Bounds GetBounds()
    {
        var size = Template.GetSize().ToWorld();
        if (_rotation is PieceRotation.East or PieceRotation.West)
        {
            (size.x, size.z) = (size.z, size.x);
        }

        return new Bounds(Vector3.zero, size);
    }

    private void OnDestroy()
    {
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
}
