using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionTarget : IEditablePieceTarget
{
    private static readonly int SweepLayerMask = ~LayerMask.GetMask("Connectors", "Anchors");

    private readonly Build _build;
    private readonly BuildSelection _buildSelection;
    private readonly Piece[] _pieces;
    private readonly Dictionary<Guid, Vector3> _initialPositions = new();
    private readonly Dictionary<Guid, PieceRotation> _initialRotations = new();
    private readonly Dictionary<Piece, bool[]> _colliderStates = new();

    private Piece _referencePiece;
    private Vector3 _selectionCenterOffset;
    private Vector3 _selectionHalfExtents;

    public SelectionTarget(Build build, BuildSelection buildSelection, IReadOnlyCollection<Piece> pieces)
    {
        _build = build;
        _buildSelection = buildSelection;
        _pieces = pieces.Where(build.IsPartOfBuild).Distinct().ToArray();
    }

    public bool CanRotate => true;
    public Piece ReferencePiece => _referencePiece;

    public void BeginMove(Piece referencePiece)
    {
        _referencePiece = _pieces.FirstOrDefault(piece => piece == referencePiece) ?? _pieces.FirstOrDefault();
        if (_referencePiece == null)
            return;

        _initialPositions.Clear();
        _initialRotations.Clear();

        foreach (var piece in _pieces)
        {
            _initialPositions[piece.Id] = piece.transform.position;
            _initialRotations[piece.Id] = piece.Rotation;
            piece.BeginDragging();
        }

        RefreshSweepData();
        CacheAndDisableColliders();
    }

    public bool TryGetMovePosition(Ray ray, out Vector3 targetPosition)
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

    public void UpdateMove(Vector3 targetPosition)
    {
        if (_referencePiece == null)
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

    public ICommand EndMove()
    {
        RestoreColliders();

        foreach (var piece in _pieces)
            piece.EndDragging();

        var transformedPieces = new Dictionary<Guid, (Vector3 StartPosition, Vector3 FinalPosition, PieceRotation StartRotation, PieceRotation FinalRotation)>();
        foreach (var piece in _pieces)
        {
            if (!_initialPositions.TryGetValue(piece.Id, out var startPosition))
                continue;
            if (!_initialRotations.TryGetValue(piece.Id, out var startRotation))
                continue;

            var finalPosition = piece.transform.position;
            var finalRotation = piece.Rotation;
            if (finalPosition == startPosition && finalRotation == startRotation)
                continue;

            transformedPieces[piece.Id] = (startPosition, finalPosition, startRotation, finalRotation);
        }

        return transformedPieces.Count == 0 ? null : new TransformPiecesCommand(_build, transformedPieces);
    }

    public void RotateClockwise()
    {
        RotateAroundPivot(90f, PieceRotation.East);
    }

    public void RotateCounterClockwise()
    {
        RotateAroundPivot(-90f, PieceRotation.West);
    }

    public ICommand Paint(PieceColor color)
    {
        var oldColors = new Dictionary<Guid, PieceColor>();

        foreach (var piece in _pieces)
        {
            if (piece.Colors[0].IsEqual(color))
                continue;

            oldColors[piece.Id] = piece.Colors[0];
            piece.TrySetColor(color, 0);
        }

        return oldColors.Count == 0 ? null : new PaintPiecesCommand(_build, oldColors, color);
    }

    public ICommand Remove()
    {
        var selectionToRestore = _buildSelection.SelectedPieceIds;
        var removedPieces = new PieceData[_pieces.Length];
        for (var i = 0; i < _pieces.Length; i++)
        {
            var piece = _pieces[i];
            removedPieces[i] = piece.GetData();
            EventBus<PieceRemovedEvent>.Raise(new PieceRemovedEvent(piece));
            _build.Remove(piece);
        }

        _buildSelection.Clear();
        return removedPieces.Length == 0 ? null : new RemovePiecesCommand(_build, removedPieces, _buildSelection, selectionToRestore);
    }

    private void CacheAndDisableColliders()
    {
        _colliderStates.Clear();

        foreach (var piece in _pieces)
        {
            var colliders = piece.GetComponentsInChildren<Collider>(true);
            var states = new bool[colliders.Length];
            for (var i = 0; i < colliders.Length; i++)
            {
                states[i] = colliders[i].enabled;
                colliders[i].enabled = false;
            }

            _colliderStates[piece] = states;
        }
    }

    private void RestoreColliders()
    {
        foreach (var (piece, states) in _colliderStates)
        {
            var colliders = piece.GetComponentsInChildren<Collider>(true);
            var colliderCount = Mathf.Min(colliders.Length, states.Length);
            for (var i = 0; i < colliderCount; i++)
                colliders[i].enabled = states[i];
        }

        _colliderStates.Clear();
    }

    private Vector3 GetSelectionPivot()
    {
        var selectionBounds = _pieces[0].GetWorldBounds();

        for (var i = 1; i < _pieces.Length; i++)
            selectionBounds.Encapsulate(_pieces[i].GetWorldBounds());

        return selectionBounds.center;
    }

    private void RotateAroundPivot(float angle, PieceRotation rotationStep)
    {
        if (_pieces.Length == 0)
            return;

        var rotation = Quaternion.AngleAxis(angle, Vector3.up);
        var pivot = GetSelectionPivot();

        foreach (var piece in _pieces)
        {
            var rotatedPosition = pivot + rotation * (piece.transform.position - pivot);
            piece.SetRotation(PieceRotationExtensions.Add(piece.Rotation, rotationStep));
            piece.MoveTo(rotatedPosition);
        }

        if (_referencePiece != null)
            RefreshSweepData();
    }

    private void RefreshSweepData()
    {
        var selectionBounds = GetSelectionBounds();
        _selectionCenterOffset = selectionBounds.center - _referencePiece.transform.position;
        _selectionHalfExtents = selectionBounds.extents;
    }

    private Bounds GetSelectionBounds()
    {
        var selectionBounds = _pieces[0].GetWorldBounds();

        for (var i = 1; i < _pieces.Length; i++)
            selectionBounds.Encapsulate(_pieces[i].GetWorldBounds());

        return selectionBounds;
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
        var castCenter = ray.origin + _selectionCenterOffset;
        var halfExtents = Vector3.Max(_selectionHalfExtents - Vector3.one * 0.002f, Vector3.one * 0.001f);

        if (!Physics.BoxCast(castCenter, halfExtents, direction, out var hit, Quaternion.identity, Mathf.Infinity, SweepLayerMask, QueryTriggerInteraction.Ignore))
        {
            targetPosition = Vector3.zero;
            return false;
        }

        targetPosition = castCenter + direction * hit.distance - _selectionCenterOffset;
        return true;
    }
}
