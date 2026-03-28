using UnityEngine;

public class PieceSelectionOutline
{
    private readonly Outline _outline;

    public PieceSelectionOutline(Piece piece, Material outlineMaterial)
    {
        _outline = piece.GetComponent<Outline>() ?? piece.gameObject.AddComponent<Outline>();
        _outline.enabled = false;
    }

    public void SetVisible(bool visible)
    {
        if (_outline != null)
            _outline.enabled = visible;
    }
}
