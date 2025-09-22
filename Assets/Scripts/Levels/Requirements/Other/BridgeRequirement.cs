using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BridgeRequirement : IBuildRequirement
{
    [SerializeField]
    private string _requirementText;

    [SerializeField]
    private float _bridgeDistance;

    private readonly HashSet<Piece> _viewedPieces = new();

    private readonly HashSet<Piece> _connectedPieces = new();
    
    public bool IsSatisfied(Build build)
    {
        _viewedPieces.Clear();
        _connectedPieces.Clear();

        foreach (var piece in build.Pieces)
        {
            if (_viewedPieces.Contains(piece))
                continue;
            
            FindConnectedPieces(piece);

            var bounds = new Bounds();
            
            foreach (var connectedPiece in _connectedPieces)
            {
                var pieceBounds = connectedPiece.GetBounds();
                pieceBounds.center = connectedPiece.transform.localPosition;
                bounds.Encapsulate(pieceBounds);
            }

            Debug.Log(bounds.extents);
            if (bounds.extents.z >= _bridgeDistance)
                return true;
            
            _connectedPieces.Clear();
        }

        return false;
    }

    private void FindConnectedPieces(Piece piece)
    {
        if (_viewedPieces.Contains(piece))
            return;
        
        _connectedPieces.Add(piece);
        _viewedPieces.Add(piece);
        
        foreach (var connectedPiece in piece.ConnectedPieces)
            FindConnectedPieces(connectedPiece);
    }

    public string GetText() => _requirementText;
}
