using KBCore.Refs;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildColorController : MonoBehaviour
{
    [Inject]
    private readonly BuildColorSelector _buildColorSelector;

    [Inject]
    private readonly BuildEditor _buildEditor;

    [Inject]
    private readonly BuildSelection _buildSelection;

    [Inject]
    private readonly ToolController _toolController;

    [SerializeField, Scene]
    private ColorSelector _colorSelector;

    private Piece[] _activePieces = Array.Empty<Piece>();
    private Dictionary<Guid, PieceColor> _interactionStartColors;
    private ColorSelectorState _currentState;
    private ColorSelectorState _interactionStartState;
    private bool _hasActiveInteraction;
    private bool _restoringState;

    private void Awake()
    {
        _toolController.ColorSelectionRequested += OnColorSelectionRequested;
        _colorSelector.ColorChanged += OnColorChanged;
        _colorSelector.InteractionFinished += OnInteractionFinished;
    }

    private void OnColorChanged(Color color, bool transparent)
    {
        if (_restoringState)
            return;

        var newState = _colorSelector.CurrentState;

        if (!_hasActiveInteraction)
        {
            _interactionStartState = _currentState;
            _interactionStartColors = CapturePieceColors(_activePieces);
            _hasActiveInteraction = true;
        }

        _buildColorSelector.SetColor(color, transparent);

        if (HasMatchingSelection())
            ApplyColorToActivePieces(new SimpleColor(color, transparent));

        _currentState = newState;
    }

    public void RestoreSelectorState(ColorSelectorState state)
    {
        _restoringState = true;
        _colorSelector.SetState(state, notifyColorChanged: false);
        _currentState = state;
        _hasActiveInteraction = false;
        _interactionStartColors = null;
        _restoringState = false;
    }

    private void OnColorSelectionRequested(IReadOnlyList<Piece> pieces)
    {
        _activePieces = pieces?
            .Where(piece => piece != null)
            .Distinct()
            .ToArray() ?? Array.Empty<Piece>();

        if (_activePieces.Length == 0)
            return;

        var initialColor = _activePieces.Length == 1
            ? _activePieces[0].Colors[0]
            : new SimpleColor(Color.white);

        var state = ColorSelectorState.FromColor(initialColor.Color, initialColor.Transparent);

        _buildColorSelector.SetColor(initialColor);
        RestoreSelectorState(state);
    }

    private void OnInteractionFinished()
    {
        if (_restoringState || !_hasActiveInteraction || _buildEditor.Build == null)
            return;

        if (!HasMatchingSelection())
        {
            _hasActiveInteraction = false;
            _interactionStartColors = null;
            return;
        }

        var finalColor = new SimpleColor(_currentState.Color, _currentState.Transparent);
        var changedColors = new Dictionary<Guid, PieceColor>();

        foreach (var piece in _activePieces)
        {
            if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
                continue;

            if (!_interactionStartColors.TryGetValue(piece.Id, out var oldColor))
                continue;

            if (oldColor.IsEqual(finalColor) && oldColor.Transparent == finalColor.Transparent)
                continue;

            changedColors[piece.Id] = oldColor;
        }

        if (changedColors.Count > 0)
        {
            var command = new PaintPiecesCommand(
                _buildEditor.Build,
                changedColors,
                finalColor,
                _buildColorSelector,
                this,
                _interactionStartState,
                _currentState);
            _buildEditor.Commit(command);
        }

        _hasActiveInteraction = false;
        _interactionStartColors = null;
    }

    private void ApplyColorToActivePieces(PieceColor color)
    {
        if (_buildEditor.Build == null)
            return;

        foreach (var piece in _activePieces)
        {
            if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
                continue;

            if (piece.Colors[0].IsEqual(color) && piece.Colors[0].Transparent == color.Transparent)
                continue;

            piece.TrySetColor(color, 0);
        }
    }

    private static Dictionary<Guid, PieceColor> CapturePieceColors(IEnumerable<Piece> pieces)
    {
        var colors = new Dictionary<Guid, PieceColor>();

        foreach (var piece in pieces)
        {
            if (piece == null)
                continue;

            colors[piece.Id] = BuildColorSelector.Clone(piece.Colors[0]);
        }

        return colors;
    }

    private bool HasMatchingSelection()
    {
        if (_activePieces.Length == 0 || !_buildSelection.HasSelection)
            return false;

        var selectedIds = _buildSelection.SelectedPieceIds.ToHashSet();
        if (selectedIds.Count != _activePieces.Length)
            return false;

        return _activePieces.All(piece => piece != null && selectedIds.Contains(piece.Id));
    }

    private void OnDestroy()
    {
        if (_toolController != null)
            _toolController.ColorSelectionRequested -= OnColorSelectionRequested;

        if (_colorSelector != null)
        {
            _colorSelector.ColorChanged -= OnColorChanged;
            _colorSelector.InteractionFinished -= OnInteractionFinished;
        }
    }
}
