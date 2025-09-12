using System;
using KBCore.Refs;
using PrimeTween;
using UnityEngine;

public class SizeRequirementIndicator : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private MeshRenderer _meshRenderer;

    [SerializeField]
    private Color _unsatisfiedColor;

    private IBuildRequirement _buildRequirement;
    public bool _isSatisfied;

    private Color _color;
    
    private MaterialPropertyBlock _materialPropertyBlock;

    private static readonly int AlphaMultiplier = Shader.PropertyToID("_AlphaMultiplier");
    private static readonly int Color = Shader.PropertyToID("_Color");
    
    private void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    public void Initialize(SizeRequirement sizeRequirement)
    {
        _buildRequirement = sizeRequirement;
        _meshRenderer.material = sizeRequirement.GetMaterial();
        _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
        _color = _materialPropertyBlock.GetColor(Color);
        SetAlpha(0);

        var bounds = sizeRequirement.SizeBounds;

        transform.localPosition = bounds.center;
        transform.localScale = bounds.size;
    }

    public void LevelStarted(LevelController levelController)
    {
        Show();
        levelController.RequirementWasSatisfied += OnRequirementSatisfied;
        levelController.RequirementWasUnsatisfied += OnRequirementUnsatisfied;
    }

    public void LevelFinished(LevelController levelController)
    {
        Hide();
        levelController.RequirementWasSatisfied -= OnRequirementSatisfied;
        levelController.RequirementWasUnsatisfied -= OnRequirementUnsatisfied;
    }

    private void Show()
    {
        Tween.Custom(0f, 1f, 1f, SetAlpha);
    }

    private void Hide()
    {
        Tween.Custom(1f, 0f, 1f, SetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        _materialPropertyBlock.SetFloat(AlphaMultiplier, alpha);
        _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private void OnRequirementSatisfied(IBuildRequirement buildRequirement)
    {
        if (_isSatisfied || _buildRequirement != buildRequirement)
            return;

        Tween.Custom(_unsatisfiedColor, _color, 1f, SetColor);
        _isSatisfied = true;
    }

    private void OnRequirementUnsatisfied(IBuildRequirement buildRequirement)
    {
        if (!_isSatisfied || _buildRequirement != buildRequirement)
            return;
        
        Tween.Custom(_color, _unsatisfiedColor, 1f, SetColor);
        _isSatisfied = false;
    }

    private void SetColor(Color color)
    {
        _materialPropertyBlock.SetColor(AlphaMultiplier, color);
        _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
