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
    private readonly CameraControlInputContext _cameraControlInputContext;

    [Inject]
    private readonly SensitivitySettings _sensitivitySettings;

    [SerializeField]
    private float _velocityAggregationWindow;

    [SerializeField, Range(1, 25)]
    private float _decelerationFactor;

    [SerializeField]
    private TweenSettings _moveToTargetTweenSettings;

    [SerializeField, Scene]
    private CinemachineOrbitalFollow _orbitalFollow;
    
    private Vector3 _lastPosition;
    private readonly Dictionary<float, Vector3> _aggregatedVelocities = new();
    private readonly List<float> _oldVelocities = new();

    private Vector3 _velocity;
    private Tween _moveToTargetTween;

    [Inject]
    private ToolController _toolController;

    [Inject]
    private SpawnerTool _spanwerTool;

    [Inject]
    private RemoverTool _removerTool;

    [Inject]
    private MoverTool _moverTool;
    
    [Inject]
    private PainterTool _painterTool;

    [Inject]
    private BuildColorSelector _buildColorSelector;
    
    private void Awake()
    {
        //_cameraControlInputContext.Enable();
        _toolController.PickTool(_spanwerTool);
        _cameraControlInputContext.CameraMoveRequested += OnMove;
        _cameraControlInputContext.CameraMoveFinished += OnMoveFinished;
    }

    public void SetTargetPosition(Vector2 position)
    {
        _moveToTargetTween = Tween.Position(transform, new Vector3(position.x, 0, position.y), _moveToTargetTweenSettings);
    }

    private void OnMove(Vector2 delta)
    {
        if (_moveToTargetTween.isAlive)
            return;
        
        var scaledDelta = delta * _sensitivitySettings.MoveSensitivity;
        var movement = Quaternion.AngleAxis(_orbitalFollow.HorizontalAxis.Value, Vector3.up) * new Vector3(scaledDelta.x, 0, scaledDelta.y);
        transform.position += movement;
        _aggregatedVelocities.TryAdd(Time.time, movement);

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

        transform.position += _velocity * Time.deltaTime;
        _velocity = Math.ExponentialDecay(_velocity, Vector3.zero, _decelerationFactor, Time.deltaTime);
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasReleasedThisFrame)
            _toolController.PickTool(_removerTool);
        if (Keyboard.current.sKey.wasReleasedThisFrame)
            _toolController.PickTool(_spanwerTool);
        if (Keyboard.current.mKey.wasReleasedThisFrame)
            _toolController.PickTool(_moverTool);
        if (Keyboard.current.pKey.wasReleasedThisFrame)
            _toolController.PickTool(_painterTool);
        if (Keyboard.current.vKey.wasReleasedThisFrame)
            _buildColorSelector.SetColor(Color.green);
    }
}
