using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class HueWheel : ValidatedMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField, Parent]
    private Canvas _canvas;
    
    [SerializeField, Self]
    private RawImage _colorImage;

    [SerializeField, Child]
    private Image _reticleImage;

    [SerializeField]
    private float _minDistance;

    [SerializeField]
    private float _reticleMagnitude;
    
    private bool _dragging;

    private RectTransform _rectTransform;

    public event Action<float> HueChanged = delegate { }; 
    
    private void Awake()
    {
        BuildColorTexture();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        MoveReticle(new Vector2(Screen.width / 2, Screen.height));
    }

    private void BuildColorTexture()
    {
        var rectangle = RectTransformUtility.PixelAdjustRect(_colorImage.rectTransform, _canvas);
        var wheelSize = Mathf.FloorToInt(Mathf.Min(rectangle.width, rectangle.height));

        var texture = new Texture2D(wheelSize, wheelSize, TextureFormat.RGBA32, false);

        var maxDistance = (wheelSize / 2f);
        var maxDistanceSquared = maxDistance * maxDistance;

        var minDistanceSquared = _minDistance * _minDistance;

        _reticleMagnitude = Mathf.Lerp(_minDistance, maxDistance, 0.5f);
        
        for (var y = 0; y < wheelSize; y++)
            for (var x = 0; x < wheelSize; x++)
            {
                var position = new Vector2(x - wheelSize / 2f, y - wheelSize / 2f);

                if (position.sqrMagnitude > maxDistanceSquared || position.sqrMagnitude < minDistanceSquared)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }
                
                var angle = Mathf.Atan2(position.x, position.y);
                if (angle < 0)
                    angle += Mathf.PI * 2f;

                var hue = Mathf.Clamp01(angle / (Mathf.PI * 2f));
                texture.SetPixel(x, y, Color.HSVToRGB(hue, 1 , 1));
            }
        
        texture.Apply();
        _colorImage.texture = texture;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
        
        MoveReticle(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
    }
    
    public void OnPointerMove(PointerEventData eventData)
    {
        if (!_dragging)
            return;
        
        MoveReticle(eventData.position);
    }

    private void MoveReticle(Vector2 reticlePosition)
    {
        var positionInRect = RectTransformUtility.PixelAdjustPoint(reticlePosition, _rectTransform, _canvas);
        var localPosition = _rectTransform.InverseTransformPoint(positionInRect);
        var magnitudeFactor = _reticleMagnitude / localPosition.magnitude;
        var position = localPosition * magnitudeFactor;
        _reticleImage.rectTransform.localPosition = position;
        
        var angle = Mathf.Atan2(position.x, position.y);
        if (angle < 0)
            angle += Mathf.PI * 2f;

        var hue = Mathf.Clamp01(angle / (Mathf.PI * 2f));
        HueChanged(hue);
    }
}
