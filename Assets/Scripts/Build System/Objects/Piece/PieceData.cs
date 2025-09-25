using System;
using UnityEngine;

[Serializable]
public class PieceData
{
    public PieceData(IPieceTemplate template, PieceTransientData transientData)
    {
        Template = template;
        TransientData = transientData;
    }

    [field: SerializeReference]
    public IPieceTemplate Template { get; private set; }
    [field: SerializeReference]
    public PieceTransientData TransientData { get; private set; }

    public override bool Equals(object obj)
    {
        return obj is PieceData data && data.TransientData.Id == TransientData.Id;
    }

    public override int GetHashCode()
    {
        return TransientData.Id.GetHashCode();
    }
}
