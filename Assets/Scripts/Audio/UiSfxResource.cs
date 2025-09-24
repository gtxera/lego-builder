using System;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "UiSfxResource", menuName = "Scriptable Objects/UiSfxResource")]
public class UiSfxResource : ScriptableObject
{
    [field: SerializeField]
    public EventReference ClickEventReference { get; private set; }
    
    [field: SerializeField]
    public EventReference ConfirmEventReference { get; private set; }
    
    [field: SerializeField]
    public EventReference ErrorEventReference { get; private set; }
    
    [field: SerializeField]
    public EventReference PlayEventReference { get; private set; }
    
    [field: SerializeField]
    public EventReference SliderEventReference { get; private set; }

    public EventReference GetEvent(UiSfxType type)
    {
        return type switch
        {
            UiSfxType.Click => ClickEventReference,
            UiSfxType.Confirm => ConfirmEventReference,
            UiSfxType.Error => ErrorEventReference,
            UiSfxType.Play => PlayEventReference,
            UiSfxType.Slider => SliderEventReference,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
