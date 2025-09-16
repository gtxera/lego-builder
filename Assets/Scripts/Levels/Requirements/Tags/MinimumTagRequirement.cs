using System.Linq;
using UnityEngine;

public class MinimumTagRequirement : IBuildRequirement
{
    [SerializeField]
    private int _count;

    [SerializeField]
    private PieceTag _tag;
    
    public bool IsSatisfied(Build build)
    {
        var count = 0;
        
        foreach (var tag in build.Pieces.SelectMany(piece => piece.Template.GetTags()))
        {
            if (tag != _tag)
                continue;

            if (++count == _count)
                return true;
        }

        return false;
    }

    public string GetText() => $"Conter pelo menos {_count} {_tag.GetName(_count)}";
}
