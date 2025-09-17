using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelProgress
{
    private readonly IEnumerable<LevelProgress> _requiredLevels;
    
    private readonly List<LevelProgress> _unlockingLevels = new();
    
    public LevelProgress(Level level, IEnumerable<LevelProgress> requiredLevels)
    {
        Level = level;
        _requiredLevels = requiredLevels;

        if (!level.RequiredLevelsToUnlock.Any())
            IsUnlocked = true;
    }

    public event Action Unlocked = delegate { };
    public event Action<BuildData> Completed = delegate { };
    
    public Level Level { get; }
    public bool IsUnlocked { get; private set; }
    public bool IsCompleted { get;  private set; }
    public BuildData BuildData { get; private set; }
    
    public void Complete(BuildData buildData)
    {
        var wasCompletedBefore = IsCompleted;
        
        IsCompleted = true;
        BuildData = buildData;
        Completed(buildData);

        if (wasCompletedBefore)
            return;

        foreach (var level in _unlockingLevels)
            level.TryUnlock();
    }

    public void AddUnlocking(LevelProgress levelProgress) => _unlockingLevels.Add(levelProgress);

    private void TryUnlock()
    {
        if (IsUnlocked)
            return;
        
        if (_requiredLevels.Any(level => !level.IsCompleted))
            return;

        IsUnlocked = true;
        Unlocked();
    }
}
