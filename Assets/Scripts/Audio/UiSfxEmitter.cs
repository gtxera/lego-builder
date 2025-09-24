using Reflex.Attributes;
using UnityEngine;

public class UiSfxEmitter : MonoBehaviour
{
    [Inject]
    private readonly UiSfxPlayer _sfxPlayer;
    
    [SerializeField]
    private UiSfxType _sfxType;

    public void Emit() => _sfxPlayer.Play(_sfxType);
}
