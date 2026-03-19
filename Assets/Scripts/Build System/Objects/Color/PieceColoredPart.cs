using System;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Rendering;

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

    public void SetColor(Color color, bool transparent, bool selected)
    {
        if (transparent != _transparent)
        {
            _transparent = transparent;
            _renderer.sharedMaterial = _pieceMaterials.GetMaterial(_transparent);
            _renderer.shadowCastingMode = _transparent ? ShadowCastingMode.Off : ShadowCastingMode.On;
        }
        
        var materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor(BaseColorPropertyId, selected ? Color.Lerp(color, Color.white, 0.35f) : color);
        
        foreach (var index in _coloredMaterialsIndexes)
            _renderer.SetPropertyBlock(materialPropertyBlock, index);
    }
}
