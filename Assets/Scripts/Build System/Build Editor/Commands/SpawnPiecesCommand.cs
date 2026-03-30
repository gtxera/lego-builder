using System.Collections.Generic;
using System.Linq;

public class SpawnPiecesCommand : ICommand
{
    private readonly Build _build;
    private readonly IReadOnlyList<PieceData> _piecesData;

    public SpawnPiecesCommand(Build build, IReadOnlyList<PieceData> piecesData)
    {
        _build = build;
        _piecesData = piecesData;
    }

    public void Commit()
    {
        foreach (var pieceData in _piecesData)
        {
            var piece = _build.GetPiece(pieceData.TransientData.Id);
            if (piece != null)
                EventBus<PieceCreatedEvent>.Raise(new PieceCreatedEvent(piece));
        }
    }

    public void Redo()
    {
        foreach (var pieceData in _piecesData)
        {
            var piece = _build.Add(pieceData);
            EventBus<PieceCreatedEvent>.Raise(new PieceCreatedEvent(piece));
        }
    }

    public void Undo()
    {
        var pieces = _build.GetPieces(_piecesData.Select(piece => piece.TransientData.Id)).ToArray();

        foreach (var piece in pieces)
        {
            EventBus<PieceRemovedEvent>.Raise(new PieceRemovedEvent(piece));
            _build.Remove(piece);
        }
    }
}
