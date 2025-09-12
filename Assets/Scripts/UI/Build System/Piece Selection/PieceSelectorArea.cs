using System;
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
    
    [SerializeField]
    private PieceSelectorButton _pieceSelectorButtonPrefab;
    
    private void Awake()
    {
        foreach (var template in _pieceTemplateDatabase.GetTemplates<BrickPieceTemplate>())
        {
            var button = Instantiate(_pieceSelectorButtonPrefab, transform);
            button.Initialize(template, _buildTemplateSelector, _piecePreviewService);
        }
    }
}
