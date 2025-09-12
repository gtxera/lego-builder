using UnityEngine;

public class LevelSelector
{
    private readonly CameraServices _cameraServices;
    private readonly LevelSelectorInputContext _inputContext;
    
    private LevelStarter _currentLevel;

    public LevelSelector(CameraServices cameraServices, LevelSelectorInputContext inputContext, LevelController levelController)
    {
        _cameraServices = cameraServices;
        _inputContext = inputContext;
        inputContext.Tapped += OnTapped;
        levelController.LevelStarted += OnLevelStarted;
        levelController.LevelFinished += OnLevelFinished;
    }

    public void Deselect()
    {
        _currentLevel?.Deselect();
        _currentLevel = null;
    }

    private void OnLevelStarted(Level _)
    {
        Deselect();
        _inputContext.Disable();
    }

    private void OnLevelFinished(Level _)
    {
        _inputContext.Enable();
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
