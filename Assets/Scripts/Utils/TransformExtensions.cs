using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TransformExtensions
{
    public static IEnumerable<Transform> Children(this Transform transform)
    {
        return transform.Cast<Transform>();
    }
}
