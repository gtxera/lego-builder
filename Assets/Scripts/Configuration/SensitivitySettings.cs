using UnityEngine;

public class SensitivitySettings
{
    public float MoveSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(MoveSensitivity), 1);
        set => PlayerPrefs.SetFloat(nameof(MoveSensitivity), value);
    }

    public float LookXSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(LookXSensitivity), 1);
        set => PlayerPrefs.SetFloat(nameof(LookXSensitivity), value);
    }
    
    public float LookYSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(LookYSensitivity), 1);
        set => PlayerPrefs.SetFloat(nameof(LookYSensitivity), value);
    }
    
    public float ZoomSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(ZoomSensitivity), 1);
        set => PlayerPrefs.SetFloat(nameof(ZoomSensitivity), value);
    }
}
