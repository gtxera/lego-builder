using System;
using UnityEngine;

public class PieceTransientData
{
    public PieceTransientData(Guid id, Vector3 position, PieceColor[] colors, PieceRotation rotation)
    {
        Id = id;
        Position = position;
        Colors = colors;
        Rotation = rotation;
    }

    public Guid Id { get; }
    public Vector3 Position { get; }
    public PieceRotation Rotation { get; }
    public PieceColor[] Colors { get; }
}
