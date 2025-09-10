using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class Build : MonoBehaviour
{
    private readonly HashSet<Piece> _pieces = new();

    [Inject]
    private BuildEditor _buildEditor;

    [Inject]
    private BuildEditorCommandStack _commandStack;

    public IReadOnlyCollection<Piece> Pieces => _pieces;

    private void Start()
    {
        _buildEditor.StartEditing(this);
        //Add(new BrickPieceTemplate());
    }

    public Piece Add(PieceData pieceData)
    {
        var pieceGameObject = new GameObject("Piece");
        var piece = pieceGameObject.AddComponent<Piece>();
        pieceGameObject.transform.SetParent(transform);
        piece.Initialize(pieceData);
        _pieces.Add(piece);

        return piece;
    }
    
    public Piece Add(IPieceTemplate template)
    {
        var pieceGameObject = new GameObject("Piece");
        var piece = pieceGameObject.AddComponent<Piece>();
        pieceGameObject.transform.SetParent(transform);
        piece.Initialize(template);
        _pieces.Add(piece);

        return piece;
    }

    public void Remove(Piece piece)
    {
        _pieces.Remove(piece);
        Destroy(piece.gameObject);
    }

    public Piece GetPiece(Guid id) => _pieces.Single(piece => piece.Id == id);

    public IEnumerable<Piece> GetPieces(IEnumerable<Guid> ids) => _pieces.Where(piece => ids.Contains(piece.Id));

    public Bounds GetBounds()
    {
        var bounds = new Bounds();

        foreach (var piece in _pieces)
        {
            var pieceBounds = piece.GetBounds();
            pieceBounds.center = piece.transform.localPosition;
            bounds.Encapsulate(pieceBounds);
        }

        return bounds;
    }
    
    private void Update()
    {
        if (Keyboard.current.zKey.wasReleasedThisFrame)
            _commandStack.Undo();
        
        if (Keyboard.current.xKey.wasReleasedThisFrame)
            _commandStack.Redo();
    }
}
