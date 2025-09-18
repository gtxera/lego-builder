using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UndoButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;
    
    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        _button.interactable = false;
        
        _button.onClick.AddListener(() => _buildEditor.Undo());
        
        _buildEditor.UndoBecameAvailable += () => _button.interactable = true;
        _buildEditor.UndoBecameUnavailable += () => _button.interactable = false;
    }
}
