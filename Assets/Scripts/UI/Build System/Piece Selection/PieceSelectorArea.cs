using System;
using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class PieceSelectorArea : MonoBehaviour
{
    [Inject]
    private readonly PiecePreviewService _piecePreviewService;
    
    [Inject]
    private readonly PieceTemplateDatabase _pieceTemplateDatabase;

    [Inject]
    private readonly BuildTemplateSelector _buildTemplateSelector;

    [Inject]
    private readonly BuildEditor _buildEditor;

    [Inject]
    private readonly BuildColorSelector _buildColorSelector;

    [Inject]
    private readonly CameraServices _cameraServices;

    [Inject]
    private readonly ToolController _toolController;
    
    [SerializeField]
    private PieceSelectorButton _pieceSelectorButtonPrefab;

    [SerializeField]
    private PieceSelectorCategoryButton _categoryButtonPrefab;

    [SerializeField]
    private RectTransform _categoryButtonsRoot;

    [SerializeField]
    private CategoryDefinition[] _categories;

    [SerializeField]
    private Color _selectedCategoryColor;

    [SerializeField]
    private Color _normalCategoryColor;

    private PieceSelectorCategoryButton _selectedCategoryButton;
    
    private RectTransform _activePanel;
    
    private void Awake()
    {
        if (_categories == null || _categories.Length == 0)
        {
            Debug.LogWarning($"{nameof(PieceSelectorArea)} has no configured categories.", this);
            return;
        }

        SetupCategories();
        SelectCategory(_categories[0]);
    }

    private void SetupCategories()
    {
        _activePanel = null;
        _selectedCategoryButton = null;

        foreach (var category in _categories)
        {
            if (category.Root == null)
                continue;

            category.Root.gameObject.SetActive(false);
            CreateCategoryButton(category);
            PopulateCategory(category);
        }
    }

    private void CreateCategoryButton(CategoryDefinition category)
    {
        var categoryButton = Instantiate(_categoryButtonPrefab, _categoryButtonsRoot);
        category.Button = categoryButton;
        categoryButton.Initialize(category.Label, () => SelectCategory(category));
        categoryButton.SetSelected(false, _selectedCategoryColor, _normalCategoryColor);
    }

    private void PopulateCategory(CategoryDefinition category)
    {
        foreach (var template in GetTemplates(category.Kind))
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, category.Root);
            button.Initialize(
                template,
                _buildTemplateSelector,
                _piecePreviewService,
                _toolController);
        }
    }

    private void SelectCategory(CategoryDefinition category)
    {
        if (category.Root == null || category.Button == null)
            return;

        if (_activePanel != null)
            _activePanel.gameObject.SetActive(false);

        if (_selectedCategoryButton != null)
            _selectedCategoryButton.SetSelected(false, _selectedCategoryColor, _normalCategoryColor);

        _activePanel = category.Root;
        _activePanel.gameObject.SetActive(true);

        _selectedCategoryButton = category.Button;
        _selectedCategoryButton.SetSelected(true, _selectedCategoryColor, _normalCategoryColor);

        EnablePreview(category.Kind);
    }

    private IEnumerable<IPieceTemplate> GetTemplates(PieceTemplateCategory kind)
    {
        switch (kind)
        {
            case PieceTemplateCategory.Brick:
                return _pieceTemplateDatabase.GetTemplates<BrickPieceTemplate>();
            case PieceTemplateCategory.Plate:
                return _pieceTemplateDatabase.GetTemplates<PlatePieceTemplate>();
            case PieceTemplateCategory.Tile:
                return _pieceTemplateDatabase.GetTemplates<TilePieceTemplate>();
            case PieceTemplateCategory.Ramp:
                return _pieceTemplateDatabase.GetTemplates<RampPieceTemplate>();
            case PieceTemplateCategory.Mesh:
                return _pieceTemplateDatabase.GetTemplates<MeshPieceTemplate>();
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    private void EnablePreview(PieceTemplateCategory kind)
    {
        switch (kind)
        {
            case PieceTemplateCategory.Brick:
                _piecePreviewService.EnablePreview<BrickPieceTemplate>();
                break;
            case PieceTemplateCategory.Plate:
                _piecePreviewService.EnablePreview<PlatePieceTemplate>();
                break;
            case PieceTemplateCategory.Tile:
                _piecePreviewService.EnablePreview<TilePieceTemplate>();
                break;
            case PieceTemplateCategory.Ramp:
                _piecePreviewService.EnablePreview<RampPieceTemplate>();
                break;
            case PieceTemplateCategory.Mesh:
                _piecePreviewService.EnablePreview<MeshPieceTemplate>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    [Serializable]
    private class CategoryDefinition
    {
        [SerializeField]
        private string _label;

        [SerializeField]
        private PieceTemplateCategory _kind;

        [SerializeField]
        private RectTransform _root;

        public string Label => _label;
        public PieceTemplateCategory Kind => _kind;
        public RectTransform Root => _root;
        public PieceSelectorCategoryButton Button { get; set; }
    }

    private enum PieceTemplateCategory
    {
        Brick,
        Plate,
        Tile,
        Ramp,
        Mesh
    }
}
