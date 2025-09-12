using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceTemplateDatabase
{
    private readonly Dictionary<Type, IEnumerable<IPieceTemplate>> _templates = new();
    
    public IEnumerable<IPieceTemplate> GetTemplates<TPieceTemplate>() where TPieceTemplate : IPieceTemplate
    {
        var templateType = typeof(TPieceTemplate);
        
        if (_templates.TryGetValue(templateType, out var templates))
            return templates;
        
        var assets = Resources.LoadAll<PieceTemplateAsset<TPieceTemplate>>("Pieces");

        templates = assets.Select(asset => asset.GetTemplate()).ToArray();
        
        _templates.Add(templateType, templates);
        return templates;
    }
}
