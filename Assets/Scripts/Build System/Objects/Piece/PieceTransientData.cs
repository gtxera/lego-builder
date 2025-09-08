using System;
using UnityEngine;

public class PieceTransientData
{
    public PieceTransientData(Guid id, Vector3 position, PieceColor[] colors)
    {
        Id = id;
        Position = position;
        Colors = colors;
    }

    public Guid Id { get; }
    public Vector3 Position { get; }
    public PieceColor[] Colors { get; }
}
