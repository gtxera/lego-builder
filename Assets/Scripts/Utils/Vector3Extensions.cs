using UnityEngine;

namespace Utils
{
    public static class Vector3Extensions
    {
        public static Vector3 Rotated(this Vector3 vector, Quaternion rotation, Vector3 pivot = default(Vector3)) {
            return rotation * (vector - pivot) + pivot;
        }
    }
}