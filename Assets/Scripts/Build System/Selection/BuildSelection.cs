using System;
using System.Collections.Generic;
using System.Linq;

public class BuildSelection : IDisposable
{
    private readonly HashSet<Guid> _selectedPieceIds = new();
    private readonly EventBinding<PieceRemovedEvent> _onPieceRemoved;

    public BuildSelection(BuildEditor buildEditor)
    {
        buildEditor.FinishedEditing += _ => Clear();
        _onPieceRemoved = new EventBinding<PieceRemovedEvent>(OnPieceRemoved);
        EventBus<PieceRemovedEvent>.Register(_onPieceRemoved);
    }

    public event Action SelectionChanged = delegate { };

    public bool HasSelection => _selectedPieceIds.Count > 0;

    public bool Contains(Piece piece) => piece != null && _selectedPieceIds.Contains(piece.Id);

    public IReadOnlyCollection<Guid> SelectedPieceIds => _selectedPieceIds.ToArray();

    public void ReplaceSelection(IEnumerable<Guid> pieceIds)
    {
        var nextSelection = pieceIds is HashSet<Guid> hashSet ? hashSet : new HashSet<Guid>(pieceIds);

        if (_selectedPieceIds.SetEquals(nextSelection))
            return;

        _selectedPieceIds.Clear();

        foreach (var pieceId in nextSelection)
            _selectedPieceIds.Add(pieceId);

        SelectionChanged();
    }

    public void Clear()
    {
        ReplaceSelection(Array.Empty<Guid>());
    }

    public IReadOnlyList<Piece> GetSelectedPieces(Build build)
    {
        if (build == null || _selectedPieceIds.Count == 0)
            return Array.Empty<Piece>();

        return build.GetPieces(_selectedPieceIds).ToArray();
    }

    private void OnPieceRemoved(PieceRemovedEvent pieceRemovedEvent)
    {
        if (!_selectedPieceIds.Remove(pieceRemovedEvent.Piece.Id))
            return;

        SelectionChanged();
    }

    public void Dispose()
    {
        EventBus<PieceRemovedEvent>.Deregister(_onPieceRemoved);
    }
}
