using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SavedPieceSetCatalogItemPlacement : ICatalogItemPlacement
{
    private readonly Build _build;
    private readonly Piece[] _pieces;
    private readonly Dictionary<Guid, Vector3> _pieceOffsets = new();
    private readonly Dictionary<Piece, bool[]> _colliderStates = new();
    private readonly Dictionary<Piece, Collider[]> _pieceColliders = new();

    private Piece _referencePiece;

    public SavedPieceSetCatalogItemPlacement(Build build, BuildData buildData, PieceColor color)
    {
        _build = build;

        var recoloredPieces = buildData.Pieces
            .Select(piece => Recolor(piece, color))
            .ToArray();

        _pieces = recoloredPieces.Select(_build.Add).ToArray();
        _referencePiece = _pieces.FirstOrDefault();

        foreach (var piece in _pieces)
            piece.BeginDragging();

        RefreshPieceOffsets();
        CacheAndDisableColliders();
    }

    public void UpdatePosition(Ray ray)
    {
        if (_referencePiece == null)
            return;

        if (!TryGetMovePosition(ray, out var targetPosition))
            return;

        var previousReferencePosition = _referencePiece.transform.position;
        var currentReferencePosition = _referencePiece.MoveTo(targetPosition);
        var delta = currentReferencePosition - previousReferencePosition;

        if (delta == Vector3.zero)
            return;

        foreach (var piece in _pieces)
        {
            if (piece == _referencePiece)
                continue;

            piece.MoveTo(piece.transform.position + delta);
        }
    }

    public void RotateClockwise()
    {
        if (_pieces.Length == 0)
            return;

        var rotation = Quaternion.AngleAxis(90f, Vector3.up);
        var pivot = GetSelectionPivot();

        foreach (var piece in _pieces)
        {
            var rotatedPosition = pivot + rotation * (piece.transform.position - pivot);
            piece.SetRotation(PieceRotationExtensions.Add(piece.Rotation, PieceRotation.East));
            piece.MoveTo(rotatedPosition);
        }

        RefreshPieceOffsets();
    }

    public ICommand Confirm()
    {
        RestoreColliders();

        foreach (var piece in _pieces)
            piece.EndDragging();

        return new SpawnPiecesCommand(_build, _pieces.Select(piece => piece.GetData()).ToArray());
    }

    public void Cancel()
    {
        RestoreColliders();

        foreach (var piece in _pieces)
        {
            piece.EndDragging();
            _build.Remove(piece);
        }
    }

    private bool TryGetMovePosition(Ray ray, out Vector3 targetPosition)
    {
        if (_referencePiece == null)
        {
            targetPosition = Vector3.zero;
            return false;
        }

        if (TryGetSweepPosition(ray, out targetPosition))
            return true;

        return _referencePiece.TryGetAnchoredPosition(ray, out targetPosition);
    }

    private Vector3 GetSelectionPivot()
    {
        var selectionBounds = _pieces[0].GetWorldBounds();

        for (var i = 1; i < _pieces.Length; i++)
            selectionBounds.Encapsulate(_pieces[i].GetWorldBounds());

        return selectionBounds.center;
    }

    private void RefreshPieceOffsets()
    {
        _pieceOffsets.Clear();

        if (_referencePiece == null)
            return;

        foreach (var piece in _pieces)
            _pieceOffsets[piece.Id] = piece.transform.position - _referencePiece.transform.position;
    }

    private void CacheAndDisableColliders()
    {
        _colliderStates.Clear();
        _pieceColliders.Clear();

        foreach (var piece in _pieces)
        {
            var colliders = piece.GetComponentsInChildren<Collider>(true);
            var states = new bool[colliders.Length];
            for (var i = 0; i < colliders.Length; i++)
            {
                states[i] = colliders[i].enabled;
                colliders[i].enabled = false;
            }

            _pieceColliders[piece] = colliders;
            _colliderStates[piece] = states;
        }
    }

    private void RestoreColliders()
    {
        foreach (var (piece, states) in _colliderStates)
        {
            if (!_pieceColliders.TryGetValue(piece, out var colliders))
                continue;

            var colliderCount = Mathf.Min(colliders.Length, states.Length);
            for (var i = 0; i < colliderCount; i++)
                colliders[i].enabled = states[i];
        }

        _colliderStates.Clear();
        _pieceColliders.Clear();
    }

    private bool TryGetSweepPosition(Ray ray, out Vector3 targetPosition)
    {
        var direction = ray.direction;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            targetPosition = Vector3.zero;
            return false;
        }

        direction.Normalize();
        var foundHit = false;
        var closestDistance = float.PositiveInfinity;

        foreach (var piece in _pieces)
        {
            if (!_pieceOffsets.TryGetValue(piece.Id, out var offset))
                continue;

            SetPieceCollidersEnabled(piece, true);
            var hitDetected = piece.TryGetSweepDistance(ray.origin + offset, direction, out var hitDistance);
            SetPieceCollidersEnabled(piece, false);

            if (!hitDetected)
                continue;

            if (hitDistance < closestDistance)
            {
                closestDistance = hitDistance;
                foundHit = true;
            }
        }

        if (!foundHit)
        {
            targetPosition = Vector3.zero;
            return false;
        }

        targetPosition = ray.origin + direction * closestDistance;
        return true;
    }

    private void SetPieceCollidersEnabled(Piece piece, bool enabled)
    {
        if (!_pieceColliders.TryGetValue(piece, out var colliders))
            return;

        if (!_colliderStates.TryGetValue(piece, out var states))
            return;

        var colliderCount = Mathf.Min(colliders.Length, states.Length);
        for (var i = 0; i < colliderCount; i++)
            colliders[i].enabled = enabled && states[i];
    }

    private static PieceData Recolor(PieceData pieceData, PieceColor color)
    {
        var transientData = pieceData.TransientData;
        var recoloredColors = transientData.Colors
            .Select(_ => BuildColorSelector.Clone(color))
            .ToArray();

        var recoloredTransientData = new PieceTransientData(
            transientData.Id,
            transientData.LocalPosition,
            recoloredColors,
            transientData.Rotation,
            transientData.CreationTime,
            transientData.WorldPosition);

        return new PieceData(pieceData.Template, recoloredTransientData);
    }
}
