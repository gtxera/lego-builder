using UnityEngine;

public class ScreenRaycaster
{
    private readonly Camera _mainCamera;

    public ScreenRaycaster()
    {
        _mainCamera = Camera.main;
    }

    public Ray ScreenToWorldRay(Vector2 screenPosition) => _mainCamera.ScreenPointToRay(screenPosition);
}
