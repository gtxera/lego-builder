using System;
using System.Collections.Generic;
using System.Linq;

public class WorldProgress
{
    private readonly IReadOnlyDictionary<Level, LevelProgress> _levels;

    public WorldProgress(World world)
    {
        World = world;

        var levels = new Dictionary<Level, LevelProgress>();

        foreach (var level in world.Levels)
        {
            var requiredLevels = levels
                .Where(kvp => level.RequiredLevelsToUnlock.Contains(kvp.Key))
                .Select(kvp => kvp.Value)
                .ToArray();

            var progress = new LevelProgress(level, requiredLevels);
            levels.Add(level, progress);
            
            foreach (var requiredLevel in requiredLevels)
                requiredLevel.AddUnlocking(progress);
        }

        _levels = levels;
    }

    public event Action Unlocked = delegate { };
    public event Action Completed = delegate { };

    public void CompleteLevel(Level level, BuildData buildData)
    {
        var progress = _levels[level];
        progress.Complete(buildData);

        if (_levels.Values.All(lvl => lvl.IsUnlocked))
        {
            IsCompleted = true;
            Completed();
        }
    }

    public LevelProgress GetProgress(Level level) => _levels[level];

    public World World { get; }
    public bool IsUnlocked { get; private set; } = true;
    public bool IsCompleted { get; private set; }
}
