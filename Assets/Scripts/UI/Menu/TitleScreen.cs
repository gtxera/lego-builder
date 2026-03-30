using System.Collections;
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
        if (_menuCamera == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (SceneTransitionState.ConsumeSkipLinearIntroOnce())
        {
            gameObject.SetActive(false);
            _menuCamera.EnterGameplayWithoutIntro();
            return;
        }

        Show();
    }

    public void Show()
    {
        _rootGroup.alpha = 1f;
        _titleImage.color = new Color(1, 1, 1, 0);
        _startText.color = new Color(1, 1, 1, 0);
        gameObject.SetActive(true);
        _playing = true;
        
        Sequence.Create()
            .Chain(Tween.Alpha(_titleImage, 1f, 2f, Ease.InCirc))
            .Chain(Tween.Alpha(_startText, 1f, 2f, Ease.InCirc))
            .OnComplete(() =>
            {
                _playing = false;
            });
    }

    private void Update()
    {
        if (_playing)
            return;

        if (Pointer.current != null && Pointer.current.press.wasReleasedThisFrame)
        {
            Tween.Alpha(_rootGroup, 0f, 1f)
                .OnComplete(target: this, screen => screen.gameObject.SetActive(false));
            _menuCamera.PlayStartAnimation();
            _playing = true;
        }
    }
}
