using KBCore.Refs;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class SaturationValuePicker : ValidatedMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField, Parent]
    private Canvas _canvas;

    [SerializeField, Self]
    private RawImage _colorImage;

    [SerializeField, Child]
    private Image _reticleImage;

    private RectTransform _rectTransform;

    private bool _dragging;

    public event Action<Vector2> SaturationValueChanged = delegate { };

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        MoveReticle(_rectTransform.position + (Vector3)(_rectTransform.rect.size / 2 * new Vector2(-1, 1)));
    }

    public void SetHue(float hue)
    {
        _colorImage.material.SetFloat("_Hue", hue);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
        MoveReticle(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!_dragging)
            return;

        MoveReticle(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
    }

    private void MoveReticle(Vector2 reticlePosition)
    {
        var positionInRect = RectTransformUtility.PixelAdjustPoint(reticlePosition, _rectTransform, _canvas);

        var localPosition = _rectTransform.InverseTransformPoint(positionInRect);
        var normalized = Rect.PointToNormalized(_rectTransform.rect, localPosition);
        var position = Vector2.Scale(normalized, _rectTransform.rect.size) - _rectTransform.rect.size / 2f;

        _reticleImage.rectTransform.localPosition = position;

        SaturationValueChanged(normalized);
    }
}
