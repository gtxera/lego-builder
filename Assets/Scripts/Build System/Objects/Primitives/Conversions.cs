using UnityEngine;

public static class Conversions
{
    public const float PieceToWorld = 0.8f;
    public const float WorldToPiece = 1.25f;
    
    public static int FromWorld(float unit) => Mathf.RoundToInt(unit * WorldToPiece);

    public static float ToWorld(int unit) => unit * PieceToWorld;
}
