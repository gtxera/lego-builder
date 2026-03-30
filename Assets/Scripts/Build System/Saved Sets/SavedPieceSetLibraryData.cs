using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedPieceSetLibraryData
{
    [SerializeReference]
    private SavedPieceSetDefinition[] _sets;

    public SavedPieceSetLibraryData(IReadOnlyList<SavedPieceSetDefinition> sets)
    {
        _sets = sets == null ? Array.Empty<SavedPieceSetDefinition>() : new SavedPieceSetDefinition[sets.Count];

        if (sets == null)
            return;

        for (var i = 0; i < sets.Count; i++)
            _sets[i] = sets[i];
    }

    public IReadOnlyList<SavedPieceSetDefinition> Sets => _sets ?? Array.Empty<SavedPieceSetDefinition>();
}
