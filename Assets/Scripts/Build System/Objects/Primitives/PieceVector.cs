using System;
using UnityEngine;

[Serializable]
public struct PieceVector : IEquatable<PieceVector>
{
    public int X;
    public int Y;
    public float Height;

    public PieceVector(int x, int y, float height = 0)
    {
        X = x;
        Y = y;
        Height = height;
    }

    public static PieceVector FromWorld(Vector3 vector3)
    {
        return new PieceVector(Conversions.FromWorld(vector3.x), Conversions.FromWorld(vector3.z), vector3.y);
    }
    
    public Vector3 ToWorld()
    {
        return new Vector3(Conversions.ToWorld(X), Height, Conversions.ToWorld(Y));
    }

    public bool Equals(PieceVector other)
    {
        return X == other.X && Y == other.Y && Mathf.Approximately(Height, other.Height);
    }

    public override bool Equals(object obj)
    {
        return obj is PieceVector other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Height);
    }
}
