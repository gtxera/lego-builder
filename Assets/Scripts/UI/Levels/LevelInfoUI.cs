using System;
using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class LevelInfoUI : MonoBehaviour
{
    [Inject]
    private readonly LevelController _levelController;

    [SerializeField]
    private LevelRequirementText _requirementTextPrefab;

    [SerializeField]
    private RectTransform _requirementsRoot;

    [SerializeField]
    private Button _completeButton;
    
    private ObjectPool<LevelRequirementText> _requirementTextPool;

    private readonly List<LevelRequirementText> _takenRequirementTexts = new();
    
    private void Awake()
    {
        _levelController.LevelStarted += OnLevelStarted;
        _levelController.LevelFinished += OnLevelFinished;
        _levelController.LevelBecameCompletable += () => _completeButton.interactable = true;
        _levelController.LevelBecameUncompletable += () => _completeButton.interactable = false;

        _requirementTextPool =
            new ObjectPool<LevelRequirementText>(CreateRequirementText, GetRequirementText, ReleaseRequirementText, DestroyRequirementText);
    }

    private void OnLevelStarted(Level level)
    {
        foreach (var requirement in level.Requirements)
        {
            if (requirement is SizeRequirement)
                continue;
            
            var requirementText = _requirementTextPool.Get();
            _takenRequirementTexts.Add(requirementText);
            requirementText.Initialize(requirement, _levelController);
        }
    }

    private void OnLevelFinished(Level _)
    {
        foreach (var requirementText in _takenRequirementTexts)
        {
            requirementText.Reset();
            _requirementTextPool.Release(requirementText);
        }
        
        _takenRequirementTexts.Clear();
    }

    private LevelRequirementText CreateRequirementText()
    {
        return Instantiate(_requirementTextPrefab, _requirementsRoot);
    }

    private static void GetRequirementText(LevelRequirementText levelRequirementText)
    {
        levelRequirementText.gameObject.SetActive(true);
    }

    private static void ReleaseRequirementText(LevelRequirementText levelRequirementText)
    {
        levelRequirementText.gameObject.SetActive(false);
    }

    private static void DestroyRequirementText(LevelRequirementText levelRequirementText)
    {
        Destroy(levelRequirementText.gameObject);
    }
}
