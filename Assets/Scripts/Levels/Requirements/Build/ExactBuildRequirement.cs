using System;
using UnityEngine;

[Serializable]
public class ExactBuildRequirement : IBuildRequirement
{
    [SerializeField]
    private JsonBuild _jsonBuild;

    public bool IsSatisfied(Build build)
    {
        if (build == null || _jsonBuild == null)
            return false;

        var requiredBuild = _jsonBuild.GetBuildData();
        if (requiredBuild == null)
            return false;

        return ExactBuildRequirementMatcher.AreEquivalent(requiredBuild, build.GetBuildData());
    }

    public string GetText()
    {
        return _jsonBuild == null
            ? "Montar exatamente a construção indicada"
            : $"Montar exatamente a construção \"{_jsonBuild.name}\"";
    }

    public BuildData GetRequiredBuildData()
    {
        return _jsonBuild == null ? null : _jsonBuild.GetBuildData();
    }
}
