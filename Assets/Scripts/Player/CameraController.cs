using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : ValidatedMonoBehaviour
{
    [Inject]
    private readonly ToolController _toolController;

    [Inject]
    private readonly CameraControlInputContext _cameraControlInputContext;

    [Inject]
    private readonly SensitivitySettings _sensitivitySettings;
    
    [Inject]
    private BuildEditor _buildEditor;

    [SerializeField]
    private float _velocityAggregationWindow;

    [SerializeField, Range(1, 25)]
    private float _decelerationFactor;

    [SerializeField]
    private TweenSettings _moveToTargetTweenSettings;

    [SerializeField, Scene]
    private CinemachineOrbitalFollow _orbitalFollow;
    
    private readonly Dictionary<float, Vector3> _aggregatedVelocities = new();
    private readonly List<float> _oldVelocities = new();
    private readonly List<Bounds> _limitingBounds = new();

    private Vector3 _velocity;
    private Tween _moveToTargetTween;

    private void Awake()
    {
        _toolController.CameraMoveRequested += OnMove;
        _toolController.CameraMoveFinished += OnMoveFinished;
        _cameraControlInputContext.CameraMoveRequested += OnMove;
        _cameraControlInputContext.CameraMoveFinished += OnMoveFinished;
        _cameraControlInputContext.CameraLookOrbitXRequested += OnLookX;
        _cameraControlInputContext.CameraLookOrbitYRequested += OnLookY;
        _cameraControlInputContext.CameraZoomRequested += OnZoom;
        
        _limitingBounds.Add(new Bounds(Vector3.zero, Vector3.one * 120f));

        Application.targetFrameRate = 60;
    }

    public void SetTargetPosition(Vector2 position)
    {
        _velocity = Vector3.zero;
        var worldPosition = new Vector3(position.x, 0, position.y);
        if (transform.position != worldPosition)
            _moveToTargetTween = Tween.Position(transform, new Vector3(position.x, 0, position.y), _moveToTargetTweenSettings);
    }

    private void OnMove(Vector2 delta)
    {
        if (_moveToTargetTween.isAlive)
            return;
        
        var scaledDelta = delta * _sensitivitySettings.MoveSensitivity;
        var velocity = Quaternion.AngleAxis(_orbitalFollow.HorizontalAxis.Value, Vector3.up) * new Vector3(scaledDelta.x, 0, scaledDelta.y);
        Move(velocity);
        _aggregatedVelocities.TryAdd(Time.time, velocity * Time.deltaTime);

        foreach (var timestamp in _aggregatedVelocities.Keys.Where(timestamp => Time.time - timestamp > _velocityAggregationWindow))
            _oldVelocities.Add(timestamp);

        foreach (var timestamp in _oldVelocities)
            _aggregatedVelocities.Remove(timestamp);
        
        _oldVelocities.Clear();
    }

    private void OnMoveFinished()
    {
        if (_moveToTargetTween.isAlive)
            return;
        
        var averageVelocity = _aggregatedVelocities
            .Where(kvp => Time.time - kvp.Key <= _velocityAggregationWindow)
            .Select(kvp => kvp.Value)
            .DefaultIfEmpty()
            .Aggregate((sum, velocity) => sum + velocity) / _velocityAggregationWindow;
        
        _velocity = averageVelocity;
        _aggregatedVelocities.Clear();
    }

    private void LateUpdate()
    {
        if (_velocity == Vector3.zero || _moveToTargetTween.isAlive)
            return;

        Move(_velocity);
        _velocity = Math.ExponentialDecay(_velocity, Vector3.zero, _decelerationFactor, Time.deltaTime);
    }

    private void Move(Vector3 velocity)
    {
        var newPosition = transform.position + velocity * Time.deltaTime;

        foreach (var limitingBounds in _limitingBounds)
        {
            if (!limitingBounds.Contains(newPosition))
                newPosition = limitingBounds.ClosestPoint(newPosition);
        }

        transform.position = newPosition;
    }

    private void Update()
    {
        if (Keyboard.current.zKey.wasReleasedThisFrame)
            _buildEditor.Undo();
        
        if (Keyboard.current.xKey.wasReleasedThisFrame)
            _buildEditor.Redo();
    }

    private void OnLookX(float delta)
    {
        _orbitalFollow.HorizontalAxis.Value += delta * _sensitivitySettings.LookXSensitivity * Time.deltaTime;
    }

    private void OnLookY(float delta)
    {
        _orbitalFollow.VerticalAxis.Value += delta * _sensitivitySettings.LookYSensitivity * Time.deltaTime;
    }

    private void OnZoom(float delta)
    {
        _orbitalFollow.RadialAxis.Value += delta * _sensitivitySettings.ZoomSensitivity * Time.deltaTime;
    }
}
