using System;
using UnityEngine;

[Serializable]
public enum PieceRotation
{
    North,
    East,
    South,
    West
}

public static class PieceRotationExtensions
{
    public static PieceRotation Add(PieceRotation lhs, PieceRotation rhs)
    {
        return (PieceRotation)(((int)lhs + (int)rhs) % 4);
    }

    public static PieceRotation Subtract(PieceRotation lhs, PieceRotation rhs)
    {
        return (PieceRotation)Mathf.Abs(((int)lhs - (int)rhs) % 4);
    }

    public static PieceVector Rotate(PieceVector vector, PieceRotation rotation)
    {
        return rotation switch
        {
            PieceRotation.North => vector,
            PieceRotation.East => new PieceVector(vector.Y, vector.X, vector.Height),
            PieceRotation.South => new PieceVector(-vector.X, -vector.Y, vector.Height),
            PieceRotation.West => new PieceVector(-vector.Y, -vector.X),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null)
        };
    }

    public static float ToAngle(this PieceRotation rotation)
    {
        return rotation switch
        {
            PieceRotation.North => 0f,
            PieceRotation.East => 90f,
            PieceRotation.South => 180f,
            PieceRotation.West => 270f,
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null)
        };
    }

    public static PieceRotation FromAngle(float angle)
    {
        var nearestAngle = Mathf.RoundToInt(Mathf.Abs(angle) / 90f) % 4;
        
        return nearestAngle switch
        {
            0 => PieceRotation.North,
            1 => PieceRotation.East,
            2 => PieceRotation.South,
            3 => PieceRotation.West,
            _ => throw new ArgumentOutOfRangeException(nameof(angle), angle, null)
        };
    }
}