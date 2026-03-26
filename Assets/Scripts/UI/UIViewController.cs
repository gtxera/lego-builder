using System;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class UIViewController : MonoBehaviour
{
    [Inject]
    private readonly ToolController _toolController;

    [SerializeField, Self]
    private CanvasGroup _canvasGroup;

    private bool _destroying;

    private void Awake()
    {
        _toolController.CameraMoveStarted += Hide;
        _toolController.CameraMoveFinished += Show;
        _toolController.SelectionMoveStarted += Hide;
        _toolController.SelectionMoveFinished += Show;
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
