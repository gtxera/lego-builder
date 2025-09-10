using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "World", menuName = "Scriptable Objects/World")]
public class World : ScriptableObject
{
    [SerializeField]
    private World[] _requiredWorldsToUnlock;
    
    [SerializeField]
    private Level[] _levels;
    
    [field: SerializeField]
    public int Index { get; private set; }
    
    [field: SerializeField]
    public string Name { get; private set; }
    
    [field: SerializeField, TextArea]
    public string Description { get; private set; }

    public IEnumerable<World> RequiredWorldsToUnlock => _requiredWorldsToUnlock;
    
    public IEnumerable<Level> Levels => _levels;
}
