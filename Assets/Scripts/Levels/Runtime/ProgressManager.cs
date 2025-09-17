using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager
{
    private readonly IReadOnlyDictionary<World, WorldProgress>
        _worlds;

    public ProgressManager()
    {
        var worlds = Resources.LoadAll<World>("Levels");
        
        var worldsDict = new Dictionary<World, WorldProgress>();
        
        foreach (var world in worlds)
        {
            var progress = new WorldProgress(world);
            worldsDict.Add(world, progress);
        }

        _worlds = worldsDict;
    }
    
    public void Complete(Level level, BuildData buildData)
    {
        _worlds[level.World].CompleteLevel(level, buildData);
    }

    public bool IsUnlocked(World world) => _worlds[world].IsUnlocked;

    public bool IsCompleted(World world) => _worlds[world].IsCompleted;
    
    public bool IsUnlocked(Level level) => GetLevelProgress(level).IsUnlocked;

    public bool IsCompleted(Level level) => GetLevelProgress(level).IsCompleted;

    public void SubscribeOnLevelUnlocked(Level level, Action onLevelUnlocked)
    {
        GetLevelProgress(level).Unlocked += onLevelUnlocked;
    }

    public void UnsubscribeOnLevelUnlocked(Level level, Action onLevelUnlocked)
    {
        GetLevelProgress(level).Unlocked -= onLevelUnlocked;
    }
    
    public void SubscribeOnLevelCompleted(Level level, Action<BuildData> onLevelCompleted)
    {
        GetLevelProgress(level).Completed += onLevelCompleted;
    }

    public void UnsubscribeOnLevelCompleted(Level level, Action<BuildData> onLevelCompleted)
    {
        GetLevelProgress(level).Completed -= onLevelCompleted;
    }
    
    private LevelProgress GetLevelProgress(Level level) => _worlds[level.World].GetProgress(level);
}
