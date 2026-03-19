public class EditablePieceTargetResolver
{
    private readonly BuildEditor _buildEditor;
    private readonly BuildSelection _buildSelection;

    public EditablePieceTargetResolver(BuildEditor buildEditor, BuildSelection buildSelection)
    {
        _buildEditor = buildEditor;
        _buildSelection = buildSelection;
    }

    public IEditablePieceTarget Resolve(Piece piece)
    {
        if (piece == null || _buildEditor.Build == null || !_buildEditor.Build.IsPartOfBuild(piece))
            return null;

        if (_buildSelection.Contains(piece))
        {
            var selectedPieces = _buildSelection.GetSelectedPieces(_buildEditor.Build);
            if (selectedPieces.Count > 1)
                return new SelectionTarget(_buildEditor.Build, _buildSelection, selectedPieces);
        }

        return new SinglePieceTarget(_buildEditor.Build, piece);
    }
}
