using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class PieceConnector<TConnector, TConnecting> : PieceConnector 
    where TConnector : PieceConnector<TConnector, TConnecting>
    where TConnecting : PieceConnector<TConnecting, TConnector>
{
    private Piece _ownerPiece;
    
    [SerializeField]
    private TConnecting _connecting;
    
    [field: SerializeField]
    public bool Connected { get; private set; }
    
    private void Awake()
    {
        var collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(.48f, .1f, .48f);
        gameObject.layer = LayerMask.NameToLayer("Connectors");
    }

    public void Initialize(Piece piece)
    {
        _ownerPiece = piece;
    }

    public sealed override bool IsOwnedBy(Piece piece) => _ownerPiece.Id == piece.Id;

    
    private void Connect(TConnecting connecting)
    {
        if (Connected || _ownerPiece.Id == connecting._ownerPiece.Id)
            return;
        
        _connecting = connecting;
        Connected = true;
        _connecting.Connect((TConnector)this);

        if (_ownerPiece.MovedMoreRecentlyThan(_connecting._ownerPiece))
        {
            var difference = _connecting.transform.position - transform.position;
            _ownerPiece.MoveTo(_ownerPiece.GetComponent<Rigidbody>().position + difference);
        }
    }

    private void Disconnect()
    {
        if (!Connected)
            return;
        
        Connected = false;
        _connecting.Disconnect();
        _connecting = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<TConnecting>(out var connecting))
            Connect(connecting);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_connecting != null && other.transform == _connecting.transform)
            Disconnect();
    }
}

public abstract class PieceConnector : MonoBehaviour
{
    public abstract bool IsOwnedBy(Piece piece);
}