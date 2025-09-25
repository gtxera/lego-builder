using System;
using KBCore.Refs;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;
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

    [FormerlySerializedAs("_audioTypeKey")]
    [SerializeField]
    private string _groupBusName;

    private FMOD.Studio.Bus _bus;

    private bool _mute;
    
    private void Awake()
    {
        _mute = Convert.ToBoolean(PlayerPrefs.GetInt(_groupBusName, 0));
        SetIcon();

        _bus = FMODUnity.RuntimeManager.GetBus($"bus:/{_groupBusName}");

        if (_mute)
            Debug.Log(_bus.setMute(true));
        
        _button.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        _mute = !_mute;
        PlayerPrefs.SetInt(_groupBusName, Convert.ToInt32(_mute));
        SetIcon();
        Debug.Log(_bus.setMute(_mute));
    }

    private void SetIcon() => _icon.sprite = _mute ? _mutedIcon : _unmutedIcon;
}
