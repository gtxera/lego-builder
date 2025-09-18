using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RedoButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;
    
    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        _button.interactable = false;

        _button.onClick.AddListener(() => _buildEditor.Redo());
        
        _buildEditor.RedoBecameAvailable += () => _button.interactable = true;
        _buildEditor.RedoBecameUnavailable += () => _button.interactable = false;
    }
}
