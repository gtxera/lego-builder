using UnityEngine;

public class PieceData
{
    public PieceData(IPieceTemplate template, PieceTransientData transientData)
    {
        Template = template;
        TransientData = transientData;
    }

    public IPieceTemplate Template { get; }
    public PieceTransientData TransientData { get; }

    public override bool Equals(object obj)
    {
        return obj is PieceData data && data.TransientData.Id == TransientData.Id;
    }

    public override int GetHashCode()
    {
        return TransientData.Id.GetHashCode();
    }
}
