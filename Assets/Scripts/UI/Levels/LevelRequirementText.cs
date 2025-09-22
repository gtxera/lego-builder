using System;
using KBCore.Refs;
using PrimeTween;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LevelRequirementText : MonoBehaviour
{
    [SerializeField, Self]
    private TextMeshProUGUI _text;

    [SerializeField]
    private Color _satisfiedColor;

    [SerializeField]
    private Color _unsatisfiedColor;

    private bool _wasSatisfied;

    private LevelController _levelController;

    private IBuildRequirement _requirement;

    private void Awake()
    {
        _text.color = _unsatisfiedColor;
    }

    public void Initialize(IBuildRequirement requirement, LevelController levelController)
    {
        _text.SetText(requirement.GetText());
        _levelController = levelController;
        _levelController.RequirementWasSatisfied += OnSatisfied;
        _levelController.RequirementWasUnsatisfied += OnUnsatisfied;
        _requirement = requirement;
    }

    public void Reset()
    {
        _levelController.RequirementWasSatisfied -= OnSatisfied;
        _levelController.RequirementWasUnsatisfied -= OnUnsatisfied;
    }

    private void OnSatisfied(IBuildRequirement requirement)
    {
        if (_wasSatisfied || _requirement != requirement)
            return;

        Tween.Color(_text, _satisfiedColor, .4f);
        _wasSatisfied = true;
    }

    private void OnUnsatisfied(IBuildRequirement requirement)
    {
        if (!_wasSatisfied || _requirement != requirement)
            return;

        Tween.Color(_text, _unsatisfiedColor, .4f);
        _wasSatisfied = false;
    }
}
