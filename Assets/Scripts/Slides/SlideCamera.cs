using PrimeTween;
using UnityEngine;

public class SlideCamera : MonoBehaviour
{
    public void SetPositionAndRotation(Vector3 position, Vector3 rotation)
    {
        Tween.Position(transform, position, .5f, Ease.InOutSine)
            .Group(Tween.Rotation(transform, rotation, .5f, Ease.InOutSine));
    }
}
