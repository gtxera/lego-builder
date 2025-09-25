using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildData
{
    [SerializeReference]
    private PieceData[] _pieces;

    public BuildData(PieceData[] pieces)
    {
        _pieces = pieces;
    }

    public IEnumerable<PieceData> Pieces => _pieces;
}
