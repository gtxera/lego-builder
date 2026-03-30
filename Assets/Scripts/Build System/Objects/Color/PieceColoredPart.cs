using System;
using Reflex.Attributes;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.Rendering;

public class PieceColoredPart : MonoBehaviour
{
    [SerializeField]
    private int[] _coloredMaterialsIndexes;

    private bool _transparent;
    
    private Renderer _renderer;
    private MaterialPropertyBlock _materialPropertyBlock;
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

    [Inject]
    private PieceMaterials _pieceMaterials;
    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();

        if (_coloredMaterialsIndexes == null || _coloredMaterialsIndexes.Length == 0)
            _coloredMaterialsIndexes = new[] { 0 };
    }

    public void SetColor(Color color, bool transparent)
    {
        if (_renderer == null)
            return;

        if (transparent != _transparent)
        {
            if (!TryResolvePieceMaterials(out var pieceMaterials))
                return;

            _transparent = transparent;
            _renderer.sharedMaterial = pieceMaterials.GetMaterial(_transparent);
            _renderer.shadowCastingMode = _transparent ? ShadowCastingMode.Off : ShadowCastingMode.On;
        }
        
        _materialPropertyBlock.Clear();
        _materialPropertyBlock.SetColor(BaseColorPropertyId, color);
        
        foreach (var index in _coloredMaterialsIndexes)
            _renderer.SetPropertyBlock(_materialPropertyBlock, index);
    }

    private bool TryResolvePieceMaterials(out PieceMaterials pieceMaterials)
    {
        if (_pieceMaterials != null)
        {
            pieceMaterials = _pieceMaterials;
            return true;
        }

        try
        {
            _pieceMaterials = gameObject.scene.GetSceneContainer().Resolve<PieceMaterials>();
        }
        catch (Exception)
        {
            _pieceMaterials = null;
        }

        pieceMaterials = _pieceMaterials;
        return pieceMaterials != null;
    }
}
