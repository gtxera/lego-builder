using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildRadialActionMenuPresenter : MonoBehaviour
{
    [SerializeField]
    private RectTransform _contentRoot;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private BuildRadialActionButton _buttonPrefab;

    [SerializeField]
    private float _radius = 120f;

    [SerializeField]
    private Vector2 _screenPadding = new(140f, 140f);

    private readonly List<BuildRadialActionButton> _buttons = new();

    public event Action<BuildRadialActionType> ActionSelected = delegate { };

    public bool IsVisible => gameObject.activeSelf;

    private void Awake()
    {
        HideImmediate();
    }

    public void Configure(IReadOnlyList<BuildRadialActionDefinition> actions)
    {
        EnsureButtonCount(actions.Count);

        for (var i = 0; i < _buttons.Count; i++)
        {
            var active = i < actions.Count;
            _buttons[i].gameObject.SetActive(active);

            if (!active)
                continue;

            var action = actions[i];
            _buttons[i].Initialize(action, OnActionSelected);
            _buttons[i].SetAnchoredPosition(GetButtonPosition(i, actions.Count));
        }
    }

    public void Show(Vector2 screenPosition)
    {
        ClampToParent(screenPosition);
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        HideImmediate();
    }

    private void HideImmediate()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void EnsureButtonCount(int count)
    {
        while (_buttons.Count < count)
        {
            var button = Instantiate(_buttonPrefab, _contentRoot);
            _buttons.Add(button);
        }
    }

    private Vector2 GetButtonPosition(int index, int count)
    {
        if (count <= 1)
            return Vector2.up * _radius;

        var angleStep = 360f / count;
        var angle = (90f - index * angleStep) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
    }

    private void ClampToParent(Vector2 screenPosition)
    {
        var parentRect = (RectTransform)transform.parent;
        var parentCanvas = parentRect.GetComponentInParent<Canvas>();
        var camera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, camera, out var localPosition);

        localPosition.x = Mathf.Clamp(localPosition.x, parentRect.rect.xMin + _screenPadding.x, parentRect.rect.xMax - _screenPadding.x);
        localPosition.y = Mathf.Clamp(localPosition.y, parentRect.rect.yMin + _screenPadding.y, parentRect.rect.yMax - _screenPadding.y);

        ((RectTransform)transform).anchoredPosition = localPosition;
    }

    private void OnActionSelected(BuildRadialActionType actionType)
    {
        ActionSelected(actionType);
    }
}
