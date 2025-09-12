using System;
using KBCore.Refs;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class RetractablePanel : ValidatedMonoBehaviour
{
    [SerializeField, Parent(Flag.ExcludeSelf)]
    private RectTransform _panelRoot;

    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private TweenSettings<Vector2> _retractAnimationSettings;

    [SerializeField]
    private TweenSettings<Vector2> _expandAnimationSettings;

    private bool _expanded;
    
    private void Awake()
    {
        _button.onClick.AddListener(ToggleRetract);
    }

    private void ToggleRetract()
    {
        if (_expanded)
            Retract();
        else
            Expand();
    }

    private void Expand()
    {
        _expanded = true;
        Tween.UIAnchoredPosition(_panelRoot, _expandAnimationSettings);
    }

    private void Retract()
    {
        _expanded = false;
        Tween.UIAnchoredPosition(_panelRoot, _retractAnimationSettings);
    }
}
