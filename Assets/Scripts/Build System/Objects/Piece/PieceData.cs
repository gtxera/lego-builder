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
}
