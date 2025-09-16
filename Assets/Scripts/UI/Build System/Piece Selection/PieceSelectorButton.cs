using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PieceSelectorButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField, Child]
    private RawImage _image;

    [SerializeField]
    private Image _selectedImage;
    
    private IPieceTemplate _template;

    private BuildTemplateSelector _buildTemplateSelector;
    
    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    public void Initialize(IPieceTemplate pieceTemplate, BuildTemplateSelector buildTemplateSelector, PiecePreviewService piecePreviewService)
    {
        _buildTemplateSelector = buildTemplateSelector;
        _template = pieceTemplate;
        _image.texture = piecePreviewService.GetPreviewTexture(_template);

        _selectedImage.enabled = _buildTemplateSelector.SelectedTemplate == pieceTemplate;

        _buildTemplateSelector.TemplateDeselected += OnTemplateDeselected;
    }

    private void OnClick()
    {
        _buildTemplateSelector.SetTemplate(_template);
        _selectedImage.enabled = true;
    }

    private void OnTemplateDeselected(IPieceTemplate template)
    {
        if (template != _template)
            return;

        _selectedImage.enabled = false;
    }
}
