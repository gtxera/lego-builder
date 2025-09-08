using UnityEngine;

public class SensitivitySettings
{
    public float MoveSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(MoveSensitivity), 10);
        set => PlayerPrefs.SetFloat(nameof(MoveSensitivity), value);
    }

    public float LookXSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(LookXSensitivity), 5000);
        set => PlayerPrefs.SetFloat(nameof(LookXSensitivity), value);
    }
    
    public float LookYSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(LookYSensitivity), 5000);
        set => PlayerPrefs.SetFloat(nameof(LookYSensitivity), value);
    }
    
    public float ZoomSensitivity
    {
        get => PlayerPrefs.GetFloat(nameof(ZoomSensitivity), 50);
        set => PlayerPrefs.SetFloat(nameof(ZoomSensitivity), value);
    }
}
