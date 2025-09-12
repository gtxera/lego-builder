using UnityEngine;

public abstract class PieceTemplateAsset<TPieceTemplate> : ScriptableObject where TPieceTemplate : IPieceTemplate
{
    [SerializeField]
    private TPieceTemplate _pieceTemplate;

    public IPieceTemplate GetTemplate() => _pieceTemplate;
}
