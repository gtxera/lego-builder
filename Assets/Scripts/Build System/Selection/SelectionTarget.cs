using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionTarget : IEditablePieceTarget
{
    private readonly Build _build;
    private readonly BuildSelection _buildSelection;
    private readonly Piece[] _pieces;
    private readonly Dictionary<Guid, Vector3> _initialPositions = new();
    private readonly Dictionary<Piece, bool[]> _colliderStates = new();

    private Piece _referencePiece;

    public SelectionTarget(Build build, BuildSelection buildSelection, IReadOnlyCollection<Piece> pieces)
    {
        _build = build;
        _buildSelection = buildSelection;
        _pieces = pieces.Where(build.IsPartOfBuild).Distinct().ToArray();
    }

    public bool CanRotate => false;
    public Piece ReferencePiece => _referencePiece;

    public void BeginMove(Piece referencePiece)
    {
        _referencePiece = referencePiece;
        _initialPositions.Clear();

        foreach (var piece in _pieces)
        {
            _initialPositions[piece.Id] = piece.transform.position;
            piece.BeginDragging();
        }

        CacheAndDisableColliders();
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

        var movedPieces = new Dictionary<Guid, (Vector3 StartPosition, Vector3 FinalPosition)>();
        foreach (var piece in _pieces)
        {
            if (!_initialPositions.TryGetValue(piece.Id, out var startPosition))
                continue;

            var finalPosition = piece.transform.position;
            if (finalPosition == startPosition)
                continue;

            movedPieces[piece.Id] = (startPosition, finalPosition);
        }

        return movedPieces.Count == 0 ? null : new MovePiecesCommand(_build, movedPieces);
    }

    public void RotateClockwise() { }

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
        var removedPieces = new PieceData[_pieces.Length];
        for (var i = 0; i < _pieces.Length; i++)
        {
            var piece = _pieces[i];
            removedPieces[i] = piece.GetData();
            EventBus<PieceRemovedEvent>.Raise(new PieceRemovedEvent(piece));
            _build.Remove(piece);
        }

        _buildSelection.Clear();
        return removedPieces.Length == 0 ? null : new RemovePiecesCommand(_build, removedPieces);
    }

    private void CacheAndDisableColliders()
    {
        _colliderStates.Clear();

        foreach (var piece in _pieces)
        {
            if (piece == _referencePiece)
                continue;

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
}
