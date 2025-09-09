using UnityEngine;

public struct PieceVector
{
    public int X { get; set; }
    public int Y { get; set; }
    public float Height { get; set; }

    public PieceVector(int x, int y, float height = 0)
    {
        X = x;
        Y = y;
        Height = height;
    }

    public static PieceVector FromWorld(Vector3 vector3)
    {
        return new PieceVector(Mathf.RoundToInt(vector3.x / .8f), Mathf.RoundToInt(vector3.z / .8f), vector3.y);
    }

    public Vector3 ToWorld()
    {
        return new Vector3(X * .8f, Height, Y * .8f);
    }
}
