using System;
using System.Linq;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [Inject]
    private readonly LevelController _levelController;

    [SerializeField]
    private Level _level;
    
    [SerializeField, Child]
    private Build _build;

    private void Start()
    {
        _levelController.Start(_level, _build);
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
                MinimumSizeRequirement min => Color.green
            };

            var gizmoColor = Gizmos.color;
            Gizmos.color = color;
            var bounds = size.SizeBounds;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = gizmoColor;
        }
    }
}
