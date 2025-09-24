using UnityEngine;

public class UiSfxPlayer
{
    private readonly UiSfxResource _sfxResource;

    public UiSfxPlayer()
    {
        _sfxResource = Resources.Load<UiSfxResource>("UiSfxResource");
    }

    public void Play(UiSfxType type)
    {
        FMODUnity.RuntimeManager.PlayOneShot(_sfxResource.GetEvent(type));
    }
}
