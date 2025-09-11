using System;
using Reflex.Attributes;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceCameraRelativeCanvas : MonoBehaviour
{
    [Inject]
    private readonly CameraServices _cameraServices;

    private void Update()
    {
        transform.rotation = _cameraServices.GetInverseCameraLookRotation();
    }
}
