using UnityEngine;

public static class Math
{
    public static Vector3 ExponentialDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
    {
        return b + (a - b) * Mathf.Exp(-decay * deltaTime);
    }
    
    public static float ExponentialDecay(float a, float b, float decay, float deltaTime)
    {
        return b + (a - b) * Mathf.Exp(-decay * deltaTime);
    }
}
