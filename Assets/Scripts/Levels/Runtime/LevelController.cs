using System;
using System.Linq;
using UnityEngine;

public class LevelController
{
    private readonly BuildEditor _buildEditor;
    private readonly ProgressManager _progressManager;

    private Level _currentLevel;
    private Build _currentBuild;

    private bool _canComplete;
    
    public LevelController(BuildEditor buildEditor, ProgressManager progressManager)
    {
        _buildEditor = buildEditor;
        _progressManager = progressManager;
    }

    public event Action<Level> LevelStarted = delegate { };
    public event Action<Level> LevelFinished = delegate { };
    public event Action LevelBecameCompletable = delegate { };
    public event Action LevelBecameUncompletable = delegate { };
    
    public void Start(Level level, Build build)
    {
        _currentLevel = level;
        _currentBuild = build;
        
        _buildEditor.StartEditing(build);
        _buildEditor.CommandCommited += VerifyLevelCompletion;
        
        LevelStarted(_currentLevel);
        
        VerifyLevelCompletion(_currentBuild);
    }

    public void Quit()
    {
        _buildEditor.CommandCommited -= VerifyLevelCompletion;
        _buildEditor.FinishEditing();
        LevelFinished(_currentLevel);
        _currentLevel = null;
        _currentBuild = null;
    }

    public void Complete()
    {
        if (!_canComplete)
            return;
        
        Quit();
        _progressManager.Complete(_currentLevel, _currentBuild.GetBuildData());
    }

    private void VerifyLevelCompletion(Build build)
    {
        var allRequirementsSatisfied = _currentLevel.Requirements.All(requirement => requirement.IsSatisfied(build));

        Debug.Log(allRequirementsSatisfied);
        
        if (_canComplete == allRequirementsSatisfied)
            return;
        
        if (_canComplete)
        {
            _canComplete = false;
            LevelBecameUncompletable();
        }
        else
        {
            _canComplete = true;
            LevelBecameCompletable();
        }
    }
}
