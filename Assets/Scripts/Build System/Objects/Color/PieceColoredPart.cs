using System;
using UnityEngine;

public class PieceColoredPart : MonoBehaviour
{
    [SerializeField]
    private int[] _coloredMaterialsIndexes;
    
    private Renderer _renderer;
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_coloredMaterialsIndexes == null || _coloredMaterialsIndexes.Length == 0)
            _coloredMaterialsIndexes = new[] { 0 };
    }

    public void SetColor(Color color)
    {
        var materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor(BaseColorPropertyId, color);
        
        foreach (var index in _coloredMaterialsIndexes)
            _renderer.SetPropertyBlock(materialPropertyBlock, index);
    }
}
