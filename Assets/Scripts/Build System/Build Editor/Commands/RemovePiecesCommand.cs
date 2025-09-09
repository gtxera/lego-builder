using System.Collections.Generic;
using System.Linq;

public class RemovePiecesCommand : ICommand
{
    private readonly Build _build;
    private readonly IReadOnlyCollection<PieceData> _removedPieces;

    public RemovePiecesCommand(Build build, IReadOnlyCollection<PieceData> removedPieces)
    {
        _build = build;
        _removedPieces = removedPieces;
    }

    public void Commit()
    {
        
    }

    public void Redo()
    {
        var pieces = _build.GetPieces(_removedPieces.Select(data => data.TransientData.Id)).ToArray();

        foreach (var piece in pieces)
            _build.Remove(piece);
    }

    public void Undo()
    {
        foreach (var removedPiece in _removedPieces)
            _build.Add(removedPiece);
    }
}
