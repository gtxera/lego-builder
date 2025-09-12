using System;
using System.Linq;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelStarter : ValidatedMonoBehaviour
{
    [Inject]
    private readonly LevelController _levelController;

    [Inject]
    private readonly LevelSelectorInputContext _inputContext;

    [SerializeField]
    private Level _level;
    
    [SerializeField, Child]
    private Build _build;

    [SerializeField, Child]
    private LevelStarterUI _ui;

    [SerializeField, Scene]
    private CameraController _cameraController;

    [SerializeField, Self]
    private BoxCollider _collider;

    private void Start()
    {
        _ui.Initialize(_level, () => _levelController.Start(_level, _build));
        _inputContext.Enable();
        gameObject.layer = LayerMask.NameToLayer("Levels");
        _levelController.LevelStarted += _ => _collider.enabled = false;
        _levelController.LevelFinished += _ => _collider.enabled = true;
    }

    public void Select()
    {
        _ui.SelectAnimation();
        var position = transform.position;
        _cameraController.SetTargetPosition(new Vector3(position.x, position.z));
    }

    public void Deselect()
    {
        _ui.DeselectAnimation();
    }

    private void OnDrawGizmos()
    {
        if (_level == null)
            return;

        var sizes = _level.Requirements.OfType<SizeRequirement>();

        foreach (var size in sizes)
        {
            var color = size switch
            {
                ExactSizeRequirement exactSizeRequirement => Color.blue,
                MaximumSizeRequirement maximumSizeRequirement => Color.red,
                MinimumSizeRequirement min => Color.green,
                _ => throw new NotImplementedException()
            };

            var gizmoColor = Gizmos.color;
            Gizmos.color = color;
            var bounds = size.SizeBounds;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = gizmoColor;
        }
    }
}
