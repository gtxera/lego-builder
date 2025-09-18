using System;
using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class TickableManager : MonoBehaviour
{
    [Inject]
    private readonly IEnumerable<ITickable> _tickables;

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        
        foreach (var tickable in _tickables)
            tickable.Tick(deltaTime);
    }
}
