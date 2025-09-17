using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class PieceSelectorArea : MonoBehaviour
{
    [Inject]
    private readonly PiecePreviewService _piecePreviewService;
    
    [Inject]
    private readonly PieceTemplateDatabase _pieceTemplateDatabase;

    [Inject]
    private readonly BuildTemplateSelector _buildTemplateSelector;
    
    [SerializeField]
    private PieceSelectorButton _pieceSelectorButtonPrefab;

    [SerializeField]
    private RectTransform _bricksRoot;

    [SerializeField]
    private RectTransform _platesRoot;

    [SerializeField]
    private RectTransform _tilesRoot;

    [SerializeField]
    private RectTransform _meshesRoot;

    [SerializeField]
    private Button _bricksButton;
    
    [SerializeField]
    private Button _platesButton;
    
    [SerializeField]
    private Button _tilesButton;
    
    [SerializeField]
    private Button _meshesButton;

    private RectTransform _activePanel;
    
    private void Awake()
    {
        _activePanel = _bricksRoot;
        _activePanel.gameObject.SetActive(true);
        
        _platesRoot.gameObject.SetActive(false);
        _tilesRoot.gameObject.SetActive(false);
        _meshesRoot.gameObject.SetActive(false);
        
        _bricksButton.onClick.AddListener(() =>
        {
            _activePanel.gameObject.SetActive(false);
            _activePanel = _bricksRoot;
            _activePanel.gameObject.SetActive(true);
            _piecePreviewService.EnablePreview<BrickPieceTemplate>();
        });
        _platesButton.onClick.AddListener(() =>
        {
            _activePanel.gameObject.SetActive(false);
            _activePanel = _platesRoot;
            _activePanel.gameObject.SetActive(true);
            _piecePreviewService.EnablePreview<PlatePieceTemplate>();
        });
        _tilesButton.onClick.AddListener(() =>
        {
            _activePanel.gameObject.SetActive(false);
            _activePanel = _tilesRoot;
            _activePanel.gameObject.SetActive(true);
            _piecePreviewService.EnablePreview<TilePieceTemplate>();
        });
        _meshesButton.onClick.AddListener(() =>
        {
            _activePanel.gameObject.SetActive(false);
            _activePanel = _meshesRoot;
            _activePanel.gameObject.SetActive(true);
            _piecePreviewService.EnablePreview<MeshPieceTemplate>();
        });
        
        foreach (var template in _pieceTemplateDatabase.GetTemplates<BrickPieceTemplate>())
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, _bricksRoot);
            button.Initialize(template, _buildTemplateSelector, _piecePreviewService);
        }
        
        foreach (var template in _pieceTemplateDatabase.GetTemplates<PlatePieceTemplate>())
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, _platesRoot);
            button.Initialize(template, _buildTemplateSelector, _piecePreviewService);
        }
        
        foreach (var template in _pieceTemplateDatabase.GetTemplates<TilePieceTemplate>())
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, _tilesRoot);
            button.Initialize(template, _buildTemplateSelector, _piecePreviewService);
        }
        
        foreach (var template in _pieceTemplateDatabase.GetTemplates<MeshPieceTemplate>())
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, _meshesRoot);
            button.Initialize(template, _buildTemplateSelector, _piecePreviewService);
        }
        
        _piecePreviewService.EnablePreview<BrickPieceTemplate>();
    }
}
