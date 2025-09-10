using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
public class Level : ScriptableObject
{
    [field: SerializeField]
    public World World { get; private set; }
    
    [SerializeField]
    private Level[] _requiredLevelsToUnlock;
    
    [SerializeReference, SubclassInstance]
    private IBuildRequirement[] _requirements;
    
    [field: SerializeField]
    public string Name { get; private set; }
    
    [field: SerializeField, TextArea]
    public string Description { get; private set; }

    public IEnumerable<Level> RequiredLevelsToUnlock => _requiredLevelsToUnlock;
    
    public IEnumerable<IBuildRequirement> Requirements => _requirements;
}
