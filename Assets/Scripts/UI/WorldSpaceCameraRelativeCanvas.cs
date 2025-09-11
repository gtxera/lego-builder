using System;
using Reflex.Attributes;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceCameraRelativeCanvas : MonoBehaviour
{
    [Inject]
    private CameraServices _cameraServices;

    [SerializeField]
    private float _size;

    private void Awake()
    {
        
    }

    private void Update()
    {
        transform.rotation = _cameraServices.GetInverseCameraLookRotation();
    }
}
