using System.Collections.Generic;
using UnityEngine;

public class PieceResourceLoader<TResource> where TResource : Object
{
    private readonly Dictionary<string, TResource> _loadedResources = new();

    public TResource Get(string name)
    {
        if (_loadedResources.TryGetValue(name, out var resource))
            return resource;

        resource = Resources.Load<TResource>($"Pieces/Meshes/{name}");
        _loadedResources[name] = resource;

        return resource;
    }

    public IEnumerable<TResource> Get(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            if (_loadedResources.TryGetValue(name, out var resource))
                yield return resource;
            
            resource = Resources.Load<TResource>($"Pieces/Meshes/{name}");
            _loadedResources[name] = resource;

            yield return resource;
        }
    }
}
