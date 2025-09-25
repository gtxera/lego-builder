using System;
using UnityEngine;

[Serializable]
public class LevelData
{
    public LevelData(BuildData buildData, bool unlocked, bool completed)
    {
        BuildData = buildData;
        Unlocked = unlocked;
        Completed = completed;
    }

    [field: SerializeReference]
    public BuildData BuildData { get; private set; }
    
    [field: SerializeField]
    public bool Unlocked { get; private set; }
    
    [field: SerializeField]
    public bool Completed { get; private set; }
}
