using UnityEngine;

public interface IBuildRequirement
{
    bool IsSatisfied(Build build);

    string GetText();
}
