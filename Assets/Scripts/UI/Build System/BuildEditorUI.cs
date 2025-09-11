using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class BuildEditorUI : MonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;

    [SerializeField]
    private Vector2 _hiddenSizeDelta;

    [SerializeField, Self]
    private CanvasGroup _canvasGroup;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _buildEditor.StartedEditing += _ => Show();
        _buildEditor.FinishedEditing += _ => Hide();
        _rectTransform = (RectTransform)transform;
    }

    private void Show()
    {
        Tween.UISizeDelta(_rectTransform, Vector2.zero, .5f, Ease.OutCirc);
    }

    private void Hide()
    {
        Tween.UISizeDelta(_rectTransform, _hiddenSizeDelta, .5f, Ease.InCirc);
    }
}
