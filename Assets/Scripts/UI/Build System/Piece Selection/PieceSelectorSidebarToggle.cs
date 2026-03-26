using KBCore.Refs;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PieceSelectorSidebarToggle : ValidatedMonoBehaviour
{
    [SerializeField, Parent(Flag.ExcludeSelf)]
    private RectTransform _panelRoot;

    [SerializeField, Self]
    private Button _button;

    [SerializeField, Child]
    private TextMeshProUGUI _label;

    [SerializeField]
    private Vector2 _expandedPosition;

    [SerializeField]
    private Vector2 _retractedPosition;

    [SerializeField]
    private float _duration = 0.35f;

    [SerializeField]
    private Ease _ease = Ease.OutCirc;

    [SerializeField]
    private bool _startExpanded = true;

    private bool _expanded;

    private void Awake()
    {
        _expanded = _startExpanded;
        _panelRoot.anchoredPosition = _expanded ? _expandedPosition : _retractedPosition;
        UpdateLabel();
        _button.onClick.AddListener(Toggle);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        _expanded = !_expanded;
        UpdateLabel();
        Tween.UIAnchoredPosition(_panelRoot, _expanded ? _expandedPosition : _retractedPosition, _duration, _ease);
    }

    private void UpdateLabel()
    {
        if (_label == null)
            return;

        _label.SetText(_expanded ? "<" : ">");
    }
}
