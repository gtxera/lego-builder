using System;
using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelStarter : ValidatedMonoBehaviour
{
    [Inject]
    private readonly LevelController _levelController;

    [Inject]
    private readonly LevelSelectorInputContext _inputContext;

    [Inject]
    private readonly ProgressManager _progressManager;

    [SerializeField]
    private Level _level;
    
    [SerializeField, Child]
    private Build _build;

    [SerializeField, Child]
    private LevelStarterUI _ui;

    [SerializeField, Scene]
    private CameraController _cameraController;

    [SerializeField, Child]
    private MeshRenderer _indicatorRenderer;

    [SerializeField]
    private SizeRequirementIndicator _sizeRequirementIndicatorPrefab;

    [SerializeField, Self]
    private BoxCollider _collider;

    private readonly List<SizeRequirementIndicator> _sizeRequirementIndicators = new();

    private MaterialPropertyBlock _materialPropertyBlock;

    private static readonly int AlphaMultiplier = Shader.PropertyToID("_AlphaMultiplier");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    
    private void Awake()
    {
        foreach (var requirement in _level.Requirements)
        {
            if (requirement is not SizeRequirement sizeRequirement)
                continue;

            var requirementIndicator = Instantiate(_sizeRequirementIndicatorPrefab, transform);
            requirementIndicator.Initialize(sizeRequirement);
            _sizeRequirementIndicators.Add(requirementIndicator);
        }
        
        _progressManager.SubscribeOnLevelCompleted(_level, OnLevelCompleted);
        _progressManager.SubscribeOnLevelUnlocked(_level, OnLevelUnlocked);

        _materialPropertyBlock = new MaterialPropertyBlock();
        _indicatorRenderer.GetPropertyBlock(_materialPropertyBlock);
        
        if (_progressManager.IsCompleted(_level))
            SetColor(Color.green);
        else if (_progressManager.IsUnlocked(_level))
            SetColor(Color.blue);
        else
        {
            SetAlpha(0f);
            _collider.enabled = false;
        }
    }

    private void Start()
    {
        _ui.Initialize(_level, () => _levelController.Start(_level, _build));
        gameObject.layer = LayerMask.NameToLayer("Levels");
        _levelController.LevelStarted += OnLevelStarted;
        _levelController.LevelFinished += OnLevelFinished;
    }

    public void Select()
    {
        _ui.SelectAnimation();
        var position = transform.position;
        _cameraController.SetTargetPosition(new Vector3(position.x, position.z));
    }

    public void Deselect()
    {
        _ui.DeselectAnimation();
    }

    private void OnLevelStarted(Level level)
    {
        if (!_progressManager.IsUnlocked(_level))
            return;
        
        _collider.enabled = false;
        Tween.Custom(1f, 0f, 1f, SetAlpha);

        if (level != _level)
            return;

        foreach (var requirementIndicator in _sizeRequirementIndicators)
            requirementIndicator.LevelStarted(_levelController);
    }

    private void OnLevelFinished(Level level)
    {
        if (!_progressManager.IsUnlocked(_level))
            return;
        
        Tween.Custom(0f, 1f, 1f, SetAlpha)
            .OnComplete(() => _collider.enabled = true);
        
        if (level != _level)
            return;

        foreach (var requirementIndicator in _sizeRequirementIndicators)
            requirementIndicator.LevelFinished(_levelController);
    }

    private void OnLevelUnlocked()
    {
        Tween.Custom(_materialPropertyBlock.GetColor(ColorId), Color.blue, 1f, SetColor);
    }
    
    private void OnLevelCompleted()
    {
        Tween.Custom(_materialPropertyBlock.GetColor(ColorId), Color.green, 1f, SetColor);
    }
    
    private void SetAlpha(float alpha)
    {
        _materialPropertyBlock.SetFloat(AlphaMultiplier, alpha);
        _indicatorRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
    
    private void SetColor(Color color)
    {
        _materialPropertyBlock.SetColor(ColorId, color);
        _indicatorRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
