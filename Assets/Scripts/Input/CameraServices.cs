using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraServices : IDisposable
{
    private readonly Camera _mainCamera;
    private readonly CinemachineImpulseSource _impulseSource;

    private readonly EventBinding<PieceCreatedEvent> _onPieceCreated;
    private readonly EventBinding<PieceMovedEvent> _onPieceMoved;

    public CameraServices()
    {
        _mainCamera = Camera.main;
        _impulseSource = _mainCamera.GetComponent<CinemachineImpulseSource>();

        _onPieceCreated = new EventBinding<PieceCreatedEvent>(Shake);
        _onPieceMoved = new EventBinding<PieceMovedEvent>(Shake);
        
        EventBus<PieceCreatedEvent>.Register(_onPieceCreated);
        EventBus<PieceMovedEvent>.Register(_onPieceMoved);
    }

    public Ray ScreenToWorldRay(Vector2 screenPosition) => _mainCamera.ScreenPointToRay(screenPosition);

    public Quaternion GetInverseCameraLookRotation() => Quaternion.LookRotation(_mainCamera.transform.forward);

    public float GetCameraRelativeSize(float relativeSize, Vector3 position)
    {
        var distance = (_mainCamera.transform.position - position).magnitude;
        return distance * relativeSize * _mainCamera.fieldOfView;
    }
    
    private void Shake()
    {
        _impulseSource.GenerateImpulse();
    }

    public void Dispose()
    {
        EventBus<PieceCreatedEvent>.Deregister(_onPieceCreated);
        EventBus<PieceMovedEvent>.Deregister(_onPieceMoved);
    }
}
