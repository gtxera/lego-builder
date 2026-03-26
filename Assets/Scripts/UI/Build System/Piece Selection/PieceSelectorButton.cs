using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PieceSelectorButton : ValidatedMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField, Child]
    private RawImage _image;

    [SerializeField]
    private Image _selectedImage;
    
    private IPieceTemplate _template;

    private BuildTemplateSelector _buildTemplateSelector;
    private ToolController _toolController;
    private RectTransform _rectTransform;
    private bool _pointerPressed;
    private bool _suppressClick;
    private bool _spawnPlacementStarted;
    
    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _button.onClick.AddListener(OnClick);
    }

    public void Initialize(
        IPieceTemplate pieceTemplate,
        BuildTemplateSelector buildTemplateSelector,
        PiecePreviewService piecePreviewService,
        ToolController toolController)
    {
        _buildTemplateSelector = buildTemplateSelector;
        _toolController = toolController;
        _template = pieceTemplate;
        _image.texture = piecePreviewService.GetPreviewTexture(_template, new Vector2Int(256, 256));

        _selectedImage.enabled = _buildTemplateSelector.SelectedTemplate == pieceTemplate;

        _buildTemplateSelector.TemplateDeselected += OnTemplateDeselected;
    }

    private void OnClick()
    {
        if (_suppressClick)
            return;

        _buildTemplateSelector.SetTemplate(_template);
        _selectedImage.enabled = true;
    }

    private void OnTemplateDeselected(IPieceTemplate template)
    {
        if (template != _template)
            return;

        _selectedImage.enabled = false;
    }

    private void OnDestroy()
    {
        if (_buildTemplateSelector != null)
            _buildTemplateSelector.TemplateDeselected -= OnTemplateDeselected;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerPressed = true;
        _suppressClick = false;
        _spawnPlacementStarted = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pointerPressed = false;
        if (!_spawnPlacementStarted)
            _suppressClick = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_pointerPressed || _toolController == null)
            return;

        if (!_spawnPlacementStarted)
        {
            if (ContainsScreenPoint(eventData.position, eventData.pressEventCamera))
                return;

            if (!_toolController.StartExternalSpawnPlacement(_template, eventData.position))
                return;

            _selectedImage.enabled = true;
            _suppressClick = true;
            _spawnPlacementStarted = true;
        }

        _toolController.UpdateExternalSpawnPlacement(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_spawnPlacementStarted)
        {
            _toolController?.FinishExternalSpawnPlacement();
            _spawnPlacementStarted = false;
            _suppressClick = false;
        }

        _pointerPressed = false;
    }

    private bool ContainsScreenPoint(Vector2 screenPosition, Camera eventCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPosition, eventCamera);
    }
}
