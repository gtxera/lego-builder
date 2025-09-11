using System;
using System.Collections.Generic;
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

    [SerializeField]
    private float _lastMovementTime;
    
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

        _colors = new PieceColor[Template.GetColorCount()];

        Id = Guid.NewGuid();

        _connectorsRoot = new GameObject("Connectors");
        var rootTransform = _connectorsRoot.transform;
        rootTransform.SetParent(transform);
        
        foreach (var socketPosition in template.GetSocketPositions())
        {
            var socketGameObject = new GameObject("Socket");
            var socketTransform = socketGameObject.transform;
            socketTransform.SetParent(rootTransform);
            socketTransform.localPosition = socketPosition;
            
            var socket = socketGameObject.AddComponent<Socket>();
            socket.Initialize(this);
            _sockets.Add(socket);
        }

        foreach (var studPosition in template.GetStudPositions())
        {
            var studGameObject = new GameObject("Stud");
            var studTransform = studGameObject.transform;
            studTransform.SetParent(rootTransform);
            studTransform.localPosition = studPosition;

            var stud = studGameObject.AddComponent<Stud>();
            stud.Initialize(this);
            _studs.Add(stud);
        }
    }
    
    public void Initialize(PieceData pieceData)
    {
        Initialize(pieceData.Template);

        var transientData = pieceData.TransientData;

        if (transientData.Id != default)
            Id = transientData.Id;
        
        transform.localPosition = transientData.Position;

        for (int i = 0; i < transientData.Colors.Length; i++)
            TrySetColor(transientData.Colors[i], i);
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
        
        var gridPosition = GetGridPosition(position);
        _rigidbody.position = gridPosition;
        _rigidbody.PublishTransform();
        
        _lastMovementTime = Time.time;
        
        foreach (var socket in _sockets)
            socket.Connect();
        foreach (var stud in _studs)
            stud.Connect();

        return gridPosition;
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

    public void RotateClockwise()
    {
        var rotation = Quaternion.AngleAxis(90f, Vector3.up);
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
        GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", color);
    }

    public bool MovedMoreRecentlyThan(Piece piece)
    {
        return _lastMovementTime > piece._lastMovementTime;
    }

    public PieceTransientData GetTransientData() => new(Id, transform.localPosition, _colors);

    public PieceData GetData() => new(Template, GetTransientData());

    public override int GetHashCode() => Id.GetHashCode();

    public Bounds GetBounds() => new Bounds(Vector3.zero, Template.GetSize().ToWorld());

    private void OnDestroy()
    {
        for (int i = 0; i < _colors.Length; i++)
        {
            if (_colors[i] is SwatchColor swatchColor)
                if (_onColorChangedCallbacks.TryGetValue(i, out var callback))
                    swatchColor.ColorChanged -= callback;
        }
    }
}
