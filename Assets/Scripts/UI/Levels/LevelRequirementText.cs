using KBCore.Refs;
using PrimeTween;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LevelRequirementText : MonoBehaviour
{
    [SerializeField, Self]
    private TextMeshProUGUI _text;

    private bool _wasSatisfied;

    private LevelController _levelController;

    private IBuildRequirement _requirement;
    
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

        Tween.Color(_text, Color.green, .4f);
        _wasSatisfied = true;
    }

    private void OnUnsatisfied(IBuildRequirement requirement)
    {
        if (!_wasSatisfied || _requirement != requirement)
            return;

        Tween.Color(_text, Color.red, .4f);
        _wasSatisfied = false;
    }
}
