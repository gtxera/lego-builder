using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildRadialActionButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TextMeshProUGUI _label;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private BuildRadialActionType _actionType;

    private void Awake()
    {
        _icon.preserveAspect = true;
    }

    public void Initialize(BuildRadialActionDefinition action, Action<BuildRadialActionType> onSelected)
    {
        _actionType = action.ActionType;
        _icon.sprite = action.Icon;
        _label.SetText(action.Label);
        _button.interactable = action.Interactable;
        _canvasGroup.alpha = action.Interactable ? 1f : 0.45f;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onSelected(_actionType));
    }

    public void SetAnchoredPosition(Vector2 anchoredPosition)
    {
        ((RectTransform)transform).anchoredPosition = anchoredPosition;
    }
}
