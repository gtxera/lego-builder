using System;
using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class ToolButtonsGroup : MonoBehaviour
{
    [Inject]
    private IEnumerable<ITool> _tools;

    [Inject]
    private ToolController _toolController;

    [Inject]
    private CameraControlInputContext _cameraControlInputContext;

    [Inject]
    private ToolInputContext _toolInputContext;
    
    [SerializeField]
    private ToolButton _toolButtonPrefab;

    private void Awake()
    {
        foreach (var tool in _tools)
        {
            var toolButton = Instantiate(_toolButtonPrefab, transform);
            toolButton.Initialize(_toolController, tool);
        }
    }
}
