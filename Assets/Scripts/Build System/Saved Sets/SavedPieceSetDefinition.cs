using System;
using UnityEngine;

[Serializable]
public class SavedPieceSetDefinition
{
    [SerializeField]
    private string _id;

    [SerializeReference]
    private BuildData _buildData;

    public SavedPieceSetDefinition(string id, BuildData buildData)
    {
        _id = id;
        _buildData = buildData;
    }

    public string Id => _id;
    public BuildData BuildData => _buildData;
}
