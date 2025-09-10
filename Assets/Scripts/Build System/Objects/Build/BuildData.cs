using System.Collections.Generic;
using UnityEngine;

public class BuildData
{
    private readonly PieceData[] _pieces;

    public BuildData(PieceData[] pieces)
    {
        _pieces = pieces;
    }

    public IEnumerable<PieceData> Pieces => _pieces;
}
