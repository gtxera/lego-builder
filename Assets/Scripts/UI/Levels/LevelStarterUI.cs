using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelStarterUI : ValidatedMonoBehaviour
{
    [Inject]
    private readonly LevelSelector _levelSelector;
    
    [SerializeField]
    private TextMeshProUGUI _levelName;

    [SerializeField]
    private TextMeshProUGUI _levelDescription;

    [SerializeField]
    private Button _startLevelButton;

    [SerializeField]
    private Button _deselectButton;

    [SerializeField, Child]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private RectTransform _uiRoot;

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    public void Initialize(Level level, Action startLevelAction)
    {
        _levelName.SetText(level.Name);
        _levelDescription.SetText(level.Description);

        _startLevelButton.onClick.AddListener(() => startLevelAction());

        _deselectButton.onClick.AddListener(() => _levelSelector.Deselect());
    }

    public void SelectAnimation()
    {
        Tween.Scale(_uiRoot, Vector3.one, .5f, Ease.OutBounce)
            .OnComplete(() => _canvasGroup.interactable = true);
    }

    public void DeselectAnimation()
    {
        _canvasGroup.interactable = false;
        Tween.Scale(_uiRoot, Vector3.zero, .5f, Ease.OutBounce);
    }
}
