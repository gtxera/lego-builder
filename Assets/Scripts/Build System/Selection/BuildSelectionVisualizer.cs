using System;

public class BuildSelectionVisualizer : IDisposable
{
    private readonly BuildEditor _buildEditor;
    private readonly BuildSelection _buildSelection;
    private readonly EventBinding<PieceCreatedEvent> _onPieceCreated;

    public BuildSelectionVisualizer(BuildEditor buildEditor, BuildSelection buildSelection)
    {
        _buildEditor = buildEditor;
        _buildSelection = buildSelection;

        _buildSelection.SelectionChanged += RefreshCurrentBuild;
        _buildEditor.StartedEditing += RefreshBuild;
        _buildEditor.FinishedEditing += ClearBuild;

        _onPieceCreated = new EventBinding<PieceCreatedEvent>(OnPieceCreated);
        EventBus<PieceCreatedEvent>.Register(_onPieceCreated);
    }

    private void OnPieceCreated(PieceCreatedEvent pieceCreatedEvent)
    {
        pieceCreatedEvent.Piece.SetSelectedVisual(_buildSelection.Contains(pieceCreatedEvent.Piece));
    }

    private void RefreshCurrentBuild()
    {
        RefreshBuild(_buildEditor.Build);
    }

    private void RefreshBuild(Build build)
    {
        if (build == null)
            return;

        foreach (var piece in build.Pieces)
            piece.SetSelectedVisual(_buildSelection.Contains(piece));
    }

    private static void ClearBuild(Build build)
    {
        if (build == null)
            return;

        foreach (var piece in build.Pieces)
            piece.SetSelectedVisual(false);
    }

    public void Dispose()
    {
        _buildSelection.SelectionChanged -= RefreshCurrentBuild;
        _buildEditor.StartedEditing -= RefreshBuild;
        _buildEditor.FinishedEditing -= ClearBuild;
        EventBus<PieceCreatedEvent>.Deregister(_onPieceCreated);
    }
}
