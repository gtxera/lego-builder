using System;
using UnityEngine;

public class PieceTransientData
{
    public PieceTransientData(Guid id, Vector3 localPosition, PieceColor[] colors, PieceRotation rotation, float creationTime, Vector3 worldPosition)
    {
        Id = id;
        LocalPosition = localPosition;
        Colors = colors;
        Rotation = rotation;
        CreationTime = creationTime;
        WorldPosition = worldPosition;
    }

    public Guid Id { get; }
    public Vector3 LocalPosition { get; }
    public Vector3 WorldPosition { get; }
    public PieceRotation Rotation { get; }
    public PieceColor[] Colors { get; }
    public float CreationTime { get; }
}
