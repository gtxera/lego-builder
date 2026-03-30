using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SavedPieceSetLibrary
{
    private const string SaveFileName = "saved-piece-sets.json";

    private readonly List<SavedPieceSetDefinition> _sets = new();

    public SavedPieceSetLibrary()
    {
        Load();
    }

    public event Action Changed = delegate { };

    public IReadOnlyList<SavedPieceSetDefinition> Sets => _sets;

    public SavedPieceSetDefinition SaveSelection(IReadOnlyList<Piece> pieces)
    {
        var buildData = new BuildData(
            pieces
                .Select(piece => piece.GetData())
                .OrderBy(data => data.TransientData.CreationTime)
                .ToArray())
            .GetCentered();

        var definition = new SavedPieceSetDefinition(Guid.NewGuid().ToString("N"), buildData);
        _sets.Add(definition);
        Persist();
        Changed();
        return definition;
    }

    public bool Remove(string id)
    {
        var removed = _sets.RemoveAll(set => set.Id == id) > 0;
        if (!removed)
            return false;

        Persist();
        Changed();
        return true;
    }

    private void Load()
    {
        _sets.Clear();

        if (!File.Exists(SaveFilePath))
            return;

        var json = File.ReadAllText(SaveFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return;

        var data = JsonUtility.FromJson<SavedPieceSetLibraryData>(json);
        if (data?.Sets == null)
            return;

        _sets.AddRange(data.Sets.Where(set => set?.BuildData != null));
    }

    private void Persist()
    {
        var directory = Path.GetDirectoryName(SaveFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var json = JsonUtility.ToJson(new SavedPieceSetLibraryData(_sets));
        File.WriteAllText(SaveFilePath, json);
    }

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);
}
