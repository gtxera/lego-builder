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
        _collider.enabled = false;
        var color = _indicatorRenderer.material.color;
        color.a = 0f;
        Tween.MaterialColor(_indicatorRenderer.material, color, 1f);

        if (level != _level)
            return;

        foreach (var requirementIndicator in _sizeRequirementIndicators)
            requirementIndicator.LevelStarted(_levelController);
    }

    private void OnLevelFinished(Level level)
    {
        _collider.enabled = true;
        var color = _indicatorRenderer.material.color;
        color.a = 1f;
        Tween.MaterialColor(_indicatorRenderer.material, color, 1f);
        
        if (level != _level)
            return;

        foreach (var requirementIndicator in _sizeRequirementIndicators)
            requirementIndicator.LevelFinished(_levelController);
    }

    private void OnLevelUnlocked()
    {
        
    }
    
    private void OnLevelCompleted()
    {
        
    }
}
