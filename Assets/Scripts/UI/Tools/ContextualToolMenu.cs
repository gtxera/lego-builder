using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public abstract class ContextualToolMenu<TTool> : MonoBehaviour where TTool : ITool
{
    [Inject]
    private readonly ToolController _toolController;

    [SerializeField]
    private TweenSettings<Vector2> _showTween;

    [SerializeField]
    private TweenSettings<Vector2> _hideTween;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _toolController.ToolSelected += OnToolSelected;
        _toolController.ToolDeselected += OnToolDeselected;
        _rectTransform = (RectTransform)transform;

        _rectTransform.anchoredPosition = _hideTween.endValue;
    }

    private void OnToolSelected(ITool tool)
    {
        if (tool is not TTool)
            return;

        transform.SetAsLastSibling();
        Tween.UIAnchoredPosition(_rectTransform, _showTween);
    }

    private void OnToolDeselected(ITool tool)
    {
        if (tool is not TTool) 
            return;

        Tween.UIAnchoredPosition(_rectTransform, _hideTween);
    }
}
