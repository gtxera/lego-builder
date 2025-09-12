using KBCore.Refs;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _rootGroup;
    
    [SerializeField]
    private Image _titleImage;

    [SerializeField]
    private TextMeshProUGUI _startText;

    [SerializeField, Scene]
    private MenuCamera _menuCamera;

    private bool _playing = true;

    private void Start()
    {
        Sequence.Create()
            .Chain(Tween.Alpha(_titleImage, 1f, 2.5f))
            .Chain(Tween.Alpha(_startText, 1f, 2.5f))
            .OnComplete(() =>
            {
                _playing = false;
            });
    }

    private void Update()
    {
        if (_playing)
            return;

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            Tween.Alpha(_rootGroup, 0f, 1f)
                .OnComplete(target: this, screen => screen.gameObject.SetActive(false));
            _menuCamera.PlayStartAnimation();
            _playing = true;
        }
    }
}
