using System;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class UIViewController : MonoBehaviour
{
    [Inject]
    private readonly CameraControlInputContext _cameraControlInputContext;

    [Inject]
    private readonly ToolInputContext _toolInputContext;

    [SerializeField, Self]
    private CanvasGroup _canvasGroup;

    private bool _destroying;

    private void Awake()
    {
        _cameraControlInputContext.CameraMoveStarted += Hide;
        _cameraControlInputContext.CameraMoveFinished += Show;

        _toolInputContext.Pressed += _ => Hide();
        _toolInputContext.Released += _ => Show();
    }

    private void Hide()
    {
        _canvasGroup.blocksRaycasts = false;
        Tween.Alpha(_canvasGroup, 0f, .3f);
    }

    private void Show()
    {
        if (!_destroying)
            Tween.Alpha(_canvasGroup, 1f, .3f).OnComplete(() => _canvasGroup.blocksRaycasts = true);
    }

    private void OnDestroy()
    {
        Tween.StopAll(_canvasGroup);
        _destroying = true;
    }
}
