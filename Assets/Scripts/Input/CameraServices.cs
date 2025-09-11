using UnityEngine;

public class CameraServices
{
    private readonly Camera _mainCamera;

    public CameraServices()
    {
        _mainCamera = Camera.main;
    }

    public Ray ScreenToWorldRay(Vector2 screenPosition) => _mainCamera.ScreenPointToRay(screenPosition);

    public Quaternion GetInverseCameraLookRotation() => Quaternion.LookRotation(_mainCamera.transform.forward);
}
