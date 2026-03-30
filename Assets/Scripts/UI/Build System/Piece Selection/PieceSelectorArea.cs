using System;
using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class PieceSelectorArea : MonoBehaviour
{
    [Inject]
    private readonly PiecePreviewService _piecePreviewService;

    [Inject]
    private readonly BuildCatalogService _buildCatalogService;

    [Inject]
    private readonly BuildTemplateSelector _buildTemplateSelector;

    [Inject]
    private readonly BuildEditor _buildEditor;

    [Inject]
    private readonly BuildSelection _buildSelection;

    [Inject]
    private readonly SavedPieceSetLibrary _savedPieceSetLibrary;

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

    private readonly List<CategoryDefinition> _runtimeCategories = new();
    private readonly List<GameObject> _savedSetEntries = new();

    private PieceSelectorCategoryButton _selectedCategoryButton;
    private RectTransform _activePanel;
    private CategoryDefinition _savedSetsCategory;
    private Button _saveSelectionButton;
    private Button _deleteSelectionButton;

    private void Awake()
    {
        if (_categories == null || _categories.Length == 0)
        {
            Debug.LogWarning($"{nameof(PieceSelectorArea)} has no configured categories.", this);
            return;
        }

        SetupCategories();
        if (_runtimeCategories.Count == 0)
            return;

        CreateSavedSetsCategory();
        SelectCategory(_runtimeCategories[0]);
        RefreshSavedSetsCategory();
        RefreshActionButtons();

        _savedPieceSetLibrary.Changed += OnSavedSetsChanged;
        _buildSelection.SelectionChanged += OnSelectionChanged;
        _buildTemplateSelector.ItemSelected += OnSelectedItemChanged;
        _buildEditor.StartedEditing += OnBuildEditorStateChanged;
        _buildEditor.FinishedEditing += OnBuildEditorStateChanged;
    }

    private void OnDestroy()
    {
        _savedPieceSetLibrary.Changed -= OnSavedSetsChanged;
        _buildSelection.SelectionChanged -= OnSelectionChanged;
        _buildTemplateSelector.ItemSelected -= OnSelectedItemChanged;
        _buildEditor.StartedEditing -= OnBuildEditorStateChanged;
        _buildEditor.FinishedEditing -= OnBuildEditorStateChanged;
    }

    private void SetupCategories()
    {
        _activePanel = null;
        _selectedCategoryButton = null;
        _runtimeCategories.Clear();

        foreach (var category in _categories)
        {
            if (category.Root == null)
                continue;

            category.Root.gameObject.SetActive(false);
            _runtimeCategories.Add(category);
            CreateCategoryButton(category);
            PopulateCategory(category);
        }
    }

    private void CreateSavedSetsCategory()
    {
        RectTransform templateRoot = null;
        foreach (var category in _runtimeCategories)
        {
            if (category.Root == null)
                continue;

            templateRoot = category.Root;
            break;
        }

        if (templateRoot == null)
            return;

        var panelsRoot = templateRoot.parent as RectTransform;
        if (panelsRoot == null)
            return;

        var templateGrid = templateRoot.GetComponent<GridLayoutGroup>();
        var rootObject = new GameObject("Saved Sets", typeof(RectTransform), typeof(GridLayoutGroup));
        var rootTransform = (RectTransform)rootObject.transform;
        rootTransform.SetParent(panelsRoot, false);
        rootTransform.anchorMin = Vector2.zero;
        rootTransform.anchorMax = Vector2.one;
        rootTransform.sizeDelta = Vector2.zero;
        rootTransform.anchoredPosition = Vector2.zero;

        var rootGrid = rootObject.GetComponent<GridLayoutGroup>();
        if (templateGrid != null)
        {
            rootGrid.cellSize = templateGrid.cellSize;
            rootGrid.spacing = templateGrid.spacing;
            rootGrid.constraint = templateGrid.constraint;
            rootGrid.constraintCount = templateGrid.constraintCount;
            rootGrid.startAxis = templateGrid.startAxis;
            rootGrid.startCorner = templateGrid.startCorner;
            rootGrid.childAlignment = templateGrid.childAlignment;
            rootGrid.padding = templateGrid.padding;
        }

        _savedSetsCategory = new CategoryDefinition("Conjuntos", BuildCatalogCategory.SavedSet, rootTransform);
        _savedSetsCategory.Root.gameObject.SetActive(false);
        _runtimeCategories.Add(_savedSetsCategory);
        CreateCategoryButton(_savedSetsCategory);
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
        foreach (var item in _buildCatalogService.GetItems(category.Kind))
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, category.Root);
            button.Initialize(item, _buildTemplateSelector, _piecePreviewService, _toolController);
        }
    }

    private void RefreshSavedSetsCategory()
    {
        if (_savedSetsCategory?.Root == null)
            return;

        foreach (var entry in _savedSetEntries)
        {
            if (entry != null)
                Destroy(entry);
        }

        _savedSetEntries.Clear();

        _saveSelectionButton = CreateActionButton("Salvar\nSelecao", OnSaveSelectionRequested);
        _deleteSelectionButton = CreateActionButton("Excluir\nConjunto", OnDeleteSelectionRequested);

        foreach (var item in _buildCatalogService.GetItems(BuildCatalogCategory.SavedSet))
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, _savedSetsCategory.Root);
            button.Initialize(item, _buildTemplateSelector, _piecePreviewService, _toolController);
            _savedSetEntries.Add(button.gameObject);
        }

        EnsureSelectedItemExists();
        RefreshActionButtons();
    }

    private Button CreateActionButton(string label, Action onClick)
    {
        var actionButton = Instantiate(_categoryButtonPrefab, _savedSetsCategory.Root);
        actionButton.Initialize(label, onClick);
        _savedSetEntries.Add(actionButton.gameObject);
        return actionButton.GetComponent<Button>();
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

        _piecePreviewService.EnablePreview(category.Kind);
    }

    private void OnSavedSetsChanged()
    {
        RefreshSavedSetsCategory();
    }

    private void OnSelectionChanged()
    {
        RefreshActionButtons();
    }

    private void OnBuildEditorStateChanged(Build build)
    {
        RefreshActionButtons();
    }

    private void OnSelectedItemChanged(IBuildCatalogItem item)
    {
        RefreshActionButtons();
    }

    private void OnSaveSelectionRequested()
    {
        var build = _buildEditor.Build;
        if (build == null)
            return;

        var selectedPieces = _buildSelection.GetSelectedPieces(build);
        if (selectedPieces.Count == 0)
            return;

        var savedSet = _savedPieceSetLibrary.SaveSelection(selectedPieces);
        var savedSetItem = _buildCatalogService.FindBySelectionId($"saved-set:{savedSet.Id}");
        if (savedSetItem != null)
            _buildTemplateSelector.SetItem(savedSetItem);

        SelectCategory(_savedSetsCategory);
    }

    private void OnDeleteSelectionRequested()
    {
        if (_buildTemplateSelector.SelectedItem is not SavedPieceSetCatalogItem savedSetItem)
            return;

        var removed = _savedPieceSetLibrary.Remove(savedSetItem.Definition.Id);
        if (!removed)
            return;

        EnsureSelectedItemExists();
    }

    private void EnsureSelectedItemExists()
    {
        var selectedItem = _buildTemplateSelector.SelectedItem;
        if (selectedItem != null && _buildCatalogService.ContainsSelection(selectedItem.SelectionId))
            return;

        var fallbackItem = _buildCatalogService.GetDefaultItem();
        if (fallbackItem != null)
            _buildTemplateSelector.SetItem(fallbackItem);
    }

    private void RefreshActionButtons()
    {
        if (_saveSelectionButton != null)
            _saveSelectionButton.interactable = _buildEditor.Build != null && _buildSelection.HasSelection;

        if (_deleteSelectionButton != null)
            _deleteSelectionButton.interactable = _buildTemplateSelector.SelectedItem is SavedPieceSetCatalogItem;
    }

    [Serializable]
    private class CategoryDefinition
    {
        [SerializeField]
        private string _label;

        [SerializeField]
        private BuildCatalogCategory _kind;

        [SerializeField]
        private RectTransform _root;

        public CategoryDefinition(string label, BuildCatalogCategory kind, RectTransform root)
        {
            _label = label;
            _kind = kind;
            _root = root;
        }

        public string Label => _label;
        public BuildCatalogCategory Kind => _kind;
        public RectTransform Root => _root;
        public PieceSelectorCategoryButton Button { get; set; }
    }
}
