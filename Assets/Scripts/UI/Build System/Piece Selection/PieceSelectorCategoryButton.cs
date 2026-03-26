using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PieceSelectorCategoryButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private TextMeshProUGUI _label;

    public void Initialize(string label, Action onClick)
    {
        _label.SetText(label);
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClick());
    }

    public void SetSelected(bool selected, Color selectedColor, Color normalColor)
    {
        if (_button.targetGraphic == null)
            return;

        _button.targetGraphic.color = selected ? selectedColor : normalColor;
    }
}
