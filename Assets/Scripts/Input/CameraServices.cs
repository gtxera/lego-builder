using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraServices : IDisposable
{
    private readonly Camera _mainCamera;
    private readonly CinemachineImpulseSource _impulseSource;

    private readonly EventBinding<PieceCreatedEvent> _onPieceCreated;
    private readonly EventBinding<PieceMovedEvent> _onPieceMoved;

    private static int _selectionVisibilityMask;

    public CameraServices()
    {
        _mainCamera = Camera.main;
        _impulseSource = _mainCamera.GetComponent<CinemachineImpulseSource>();

        _onPieceCreated = new EventBinding<PieceCreatedEvent>(Shake);
        _onPieceMoved = new EventBinding<PieceMovedEvent>(Shake);

        if (_selectionVisibilityMask == 0)
            _selectionVisibilityMask = ~LayerMask.GetMask("Connectors", "Anchors");
        
        EventBus<PieceCreatedEvent>.Register(_onPieceCreated);
        EventBus<PieceMovedEvent>.Register(_onPieceMoved);
    }

    public Ray ScreenToWorldRay(Vector2 screenPosition) => _mainCamera.ScreenPointToRay(screenPosition);
    public Vector3 WorldToScreenPoint(Vector3 worldPosition) => _mainCamera.WorldToScreenPoint(worldPosition);

    public Quaternion GetInverseCameraLookRotation() => Quaternion.LookRotation(_mainCamera.transform.forward);

    public float GetCameraRelativeSize(float relativeSize, Vector3 position)
    {
        var distance = (_mainCamera.transform.position - position).magnitude;
        return distance * relativeSize * _mainCamera.fieldOfView;
    }

    public Vector2 WorldPositionInScreen(Vector3 position) => _mainCamera.WorldToScreenPoint(position) - new Vector3(Screen.currentResolution.width / 2f, Screen.currentResolution.height/ 2f, 0f);

    public bool TryGetScreenRect(Bounds worldBounds, out Rect screenRect)
    {
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);

        foreach (var corner in GetBoundsCorners(worldBounds))
        {
            var screenPoint = _mainCamera.WorldToScreenPoint(corner);
            if (screenPoint.z < 0f)
            {
                screenRect = default;
                return false;
            }

            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    public bool IsPieceVisibleInScreenRect(Piece piece, Rect selectionRect)
    {
        var bounds = piece.GetWorldBounds();
        var maxDistance = Vector3.Distance(_mainCamera.transform.position, bounds.center) + bounds.extents.magnitude + 0.1f;

        foreach (var samplePoint in GetVisibilitySamplePoints(bounds))
        {
            var screenPoint = _mainCamera.WorldToScreenPoint(samplePoint);
            if (screenPoint.z <= 0f)
                continue;

            var point = new Vector2(screenPoint.x, screenPoint.y);
            if (!selectionRect.Contains(point))
                continue;

            var ray = _mainCamera.ScreenPointToRay(point);
            if (!Physics.Raycast(ray, out var hit, maxDistance, _selectionVisibilityMask, QueryTriggerInteraction.Ignore))
                continue;

            var hitPiece = hit.transform.GetComponentInParent<Piece>();
            if (hitPiece == piece)
                return true;
        }

        return false;
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

    private static IEnumerable<Vector3> GetBoundsCorners(Bounds bounds)
    {
        var min = bounds.min;
        var max = bounds.max;

        yield return new Vector3(min.x, min.y, min.z);
        yield return new Vector3(min.x, min.y, max.z);
        yield return new Vector3(min.x, max.y, min.z);
        yield return new Vector3(min.x, max.y, max.z);
        yield return new Vector3(max.x, min.y, min.z);
        yield return new Vector3(max.x, min.y, max.z);
        yield return new Vector3(max.x, max.y, min.z);
        yield return new Vector3(max.x, max.y, max.z);
    }

    private static IEnumerable<Vector3> GetVisibilitySamplePoints(Bounds bounds)
    {
        yield return bounds.center;

        foreach (var corner in GetBoundsCorners(bounds))
            yield return corner;

        var center = bounds.center;
        var extents = bounds.extents;

        yield return center + new Vector3(extents.x, 0f, 0f);
        yield return center - new Vector3(extents.x, 0f, 0f);
        yield return center + new Vector3(0f, extents.y, 0f);
        yield return center - new Vector3(0f, extents.y, 0f);
        yield return center + new Vector3(0f, 0f, extents.z);
        yield return center - new Vector3(0f, 0f, extents.z);
    }
}
