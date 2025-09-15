using System;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceCameraRelativeCanvas : MonoBehaviour
{
    [Inject]
    private readonly CameraServices _cameraServices;

    [SerializeField]
    private float _relativeSize;

    private void Update()
    {
        transform.rotation = _cameraServices.GetInverseCameraLookRotation();
        transform.localScale = Vector3.one * _cameraServices.GetCameraRelativeSize(_relativeSize, transform.position);
    }

    public void SetRelativeSize(float relativeSize)
    {
        _relativeSize = relativeSize;
    }
}
