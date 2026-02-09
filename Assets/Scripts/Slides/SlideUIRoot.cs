using System;
using KBCore.Refs;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SlideUIRoot : MonoBehaviour
{
    [SerializeField, Self]
    private CanvasGroup _group;

    private void Awake()
    {
        _group.alpha = 0f;
    }

    public void Show()
    {
        Tween.Alpha(_group, 1f, .5f, Ease.InOutSine);
    }

    public void Hide()
    {
        Tween.Alpha(_group, 0f, .5f, Ease.InOutSine);
    }
}
