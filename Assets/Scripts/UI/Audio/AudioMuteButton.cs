using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class AudioMuteButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField, Child(Flag.ExcludeSelf)]
    private Image _icon;

    [SerializeField]
    private Sprite _mutedIcon;

    [SerializeField]
    private Sprite _unmutedIcon;

    [SerializeField]
    private FMODUnity.StudioEventEmitter _muteSnapshot;

    [SerializeField]
    private string _audioTypeKey;

    private bool _mute;
    
    private void Awake()
    {
        _mute = Convert.ToBoolean(PlayerPrefs.GetInt(_audioTypeKey, 0));
        SetIcon();
        
        _button.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        _muteSnapshot.Play();
        _mute = !_mute;
        PlayerPrefs.SetInt(_audioTypeKey, Convert.ToInt32(_mute));
        SetIcon();
    }

    private void SetIcon() => _icon.sprite = _mute ? _mutedIcon : _unmutedIcon;
}
