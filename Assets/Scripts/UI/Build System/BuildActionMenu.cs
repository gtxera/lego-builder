using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildActionMenu : IDisposable
{
    private const string PresenterResourcePath = "BuildRadialActionMenu";

    private readonly BuildRadialActionMenuPresenter _presenter;

    public BuildActionMenu()
    {
        var presenterPrefab = Resources.Load<BuildRadialActionMenuPresenter>(PresenterResourcePath);
        if (presenterPrefab == null)
            throw new InvalidOperationException($"Prefab do menu radial não encontrado em Resources/{PresenterResourcePath}");

        _presenter = UnityEngine.Object.Instantiate(presenterPrefab, GetMenuParent(), false);
        _presenter.gameObject.name = presenterPrefab.gameObject.name;
        _presenter.ActionSelected += OnActionSelected;
    }

    public event Action ColorRequested = delegate { };
    public event Action RotateRightRequested = delegate { };
    public event Action RotateLeftRequested = delegate { };
    public event Action RemoveRequested = delegate { };

    public bool IsVisible => _presenter != null && _presenter.IsVisible;

    public void Show(Vector2 screenPosition, bool hasSelection)
    {
        var actions = new[]
        {
            new BuildRadialActionDefinition(BuildRadialActionType.Color, "Colorir", Resources.Load<Sprite>("Icons/Brush"), hasSelection),
            new BuildRadialActionDefinition(BuildRadialActionType.RotateRight, "Rotacionar direita", null, hasSelection),
            new BuildRadialActionDefinition(BuildRadialActionType.RotateLeft, "Rotacionar esquerda", null, hasSelection),
            new BuildRadialActionDefinition(BuildRadialActionType.Remove, "Remover", Resources.Load<Sprite>("Icons/Remove"), hasSelection)
        };

        _presenter.Configure(actions);
        _presenter.Show(screenPosition);
    }

    public void Hide()
    {
        _presenter.Hide();
    }

    public bool ContainsScreenPoint(Vector2 screenPosition)
    {
        return _presenter != null && _presenter.ContainsScreenPoint(screenPosition);
    }

    public void Dispose()
    {
        if (_presenter == null)
            return;

        _presenter.ActionSelected -= OnActionSelected;
        UnityEngine.Object.Destroy(_presenter.gameObject);
    }

    private void OnActionSelected(BuildRadialActionType actionType)
    {
        switch (actionType)
        {
            case BuildRadialActionType.Color:
                ColorRequested();
                break;
            case BuildRadialActionType.RotateRight:
                RotateRightRequested();
                break;
            case BuildRadialActionType.RotateLeft:
                RotateLeftRequested();
                break;
            case BuildRadialActionType.Remove:
                RemoveRequested();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }

    private static Transform GetMenuParent()
    {
        var buildEditorUi = UnityEngine.Object.FindFirstObjectByType<BuildEditorUI>();
        if (buildEditorUi != null)
        {
            var buildEditorCanvas = buildEditorUi.GetComponentInParent<Canvas>();
            if (buildEditorCanvas != null)
                return buildEditorCanvas.transform;
        }

        var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
            return canvas.transform;

        throw new InvalidOperationException("Nenhum Canvas encontrado para anexar o menu radial.");
    }
}

public readonly struct BuildRadialActionDefinition
{
    public BuildRadialActionDefinition(BuildRadialActionType actionType, string label, Sprite icon, bool interactable)
    {
        ActionType = actionType;
        Label = label;
        Icon = icon;
        Interactable = interactable;
    }

    public BuildRadialActionType ActionType { get; }
    public string Label { get; }
    public Sprite Icon { get; }
    public bool Interactable { get; }
}

public enum BuildRadialActionType
{
    Color,
    RotateRight,
    RotateLeft,
    Remove
}
