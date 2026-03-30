using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WorldSelectionEntry : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private TMP_Text _label;

    private UnityAction _onClick;

    public void Initialize(string label, bool interactable, Action onClick)
    {
        if (_onClick != null)
            _button.onClick.RemoveListener(_onClick);

        _label.SetText(label);
        _button.interactable = interactable;

        _onClick = () => onClick?.Invoke();
        _button.onClick.AddListener(_onClick);
    }

    private void OnDestroy()
    {
        if (_onClick != null)
            _button.onClick.RemoveListener(_onClick);
    }
}
