using UnityEngine;

public class LevelSelector
{
    private readonly CameraServices _cameraServices;

    private LevelStarter _currentLevel;

    public LevelSelector(CameraServices cameraServices, LevelSelectorInputContext inputContext, LevelController levelController)
    {
        _cameraServices = cameraServices;
        inputContext.Tapped += OnTapped;
        levelController.LevelStarted += _ => Deselect();
    }

    public void Deselect()
    {
        _currentLevel?.Deselect();
        _currentLevel = null;
    }

    private void OnTapped(Vector2 tapPosition)
    {
        var ray = _cameraServices.ScreenToWorldRay(tapPosition);
        if (!Physics.Raycast(ray, out var hit, float.MaxValue, LayerMask.GetMask("Levels")))
        {
            Deselect();
            return;
        }

        if (hit.transform.TryGetComponent<LevelStarter>(out var newLevel))
        {
            if (newLevel != _currentLevel)
            {
                _currentLevel?.Deselect();
                _currentLevel = newLevel;

                if (_currentLevel != null)
                {
                    _currentLevel.Select();
                }
            }
        }
    }
}
