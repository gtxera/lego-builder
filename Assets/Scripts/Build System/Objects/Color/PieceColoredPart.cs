using System;
using Reflex.Attributes;
using UnityEngine;

public class PieceColoredPart : MonoBehaviour
{
    [SerializeField]
    private int[] _coloredMaterialsIndexes;

    private bool _transparent;
    
    private Renderer _renderer;
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

    [Inject]
    private readonly PieceMaterials _pieceMaterials;
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_coloredMaterialsIndexes == null || _coloredMaterialsIndexes.Length == 0)
            _coloredMaterialsIndexes = new[] { 0 };
    }

    public void SetColor(Color color, bool transparent)
    {
        if (transparent != _transparent)
        {
            _transparent = transparent;
            _renderer.sharedMaterial = _pieceMaterials.GetMaterial(_transparent);
        }
        
        var materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor(BaseColorPropertyId, color);
        
        foreach (var index in _coloredMaterialsIndexes)
            _renderer.SetPropertyBlock(materialPropertyBlock, index);
    }
}
