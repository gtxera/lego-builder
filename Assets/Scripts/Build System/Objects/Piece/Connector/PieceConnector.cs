using System;
using UnityEngine;

public abstract class PieceConnector<TConnector, TConnecting> : PieceConnector 
    where TConnector : PieceConnector<TConnector, TConnecting>
    where TConnecting : PieceConnector<TConnecting, TConnector>
{
    private Piece _ownerPiece;

    private Collider[] _castResults = new Collider[2];
    
    protected abstract string Layer { get; }
    
    [SerializeField]
    private TConnecting _connecting;
    
    [field: SerializeField]
    public bool Connected { get; private set; }
    
    private void Awake()
    {
        var collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.24f;
        gameObject.layer = LayerMask.NameToLayer(Layer);
    }

    public override void Initialize(Piece piece)
    {
        _ownerPiece = piece;
    }

    public sealed override bool IsOwnedBy(Piece piece) => _ownerPiece.Id == piece.Id;

    public sealed override bool IsConnectedTo(Piece piece) => _connecting?._ownerPiece.Id == piece.Id;

    public void Connect()
    {
        var size = Physics.OverlapSphereNonAlloc(transform.position - new Vector3(0, .01f, 0), .24f, _castResults, LayerMask.GetMask(Layer), QueryTriggerInteraction.Collide);
        for (int i = 0; i < size; i++)
        {
            var collider = _castResults[i];
            if (collider.gameObject == gameObject)
                continue;

            if (collider.gameObject.TryGetComponent<TConnecting>(out var connecting))
                Connect(connecting);
        }
    }
    
    private void Connect(TConnecting connecting)
    {
        if (Connected || _ownerPiece?.Id == connecting._ownerPiece?.Id || !CanConnect(connecting))
            return;
        
        _connecting = connecting;
        Connected = true;
        _connecting.Connect((TConnector)this);

        if (_ownerPiece != null && (_connecting._ownerPiece == null || _ownerPiece.MovedMoreRecentlyThan(_connecting._ownerPiece)))
        {
            var difference = _connecting.transform.position - transform.position;
            _ownerPiece.MoveDifference(difference);
        }
    }

    protected virtual bool CanConnect(TConnecting connecting) => true;

    public void Disconnect()
    {
        if (!Connected)
            return;
        
        Connected = false;
        _connecting.Disconnect();
        _connecting = null;
    }
}

public abstract class PieceConnector : MonoBehaviour
{
    public abstract bool IsOwnedBy(Piece piece);
    public abstract bool IsConnectedTo(Piece piece);

    public abstract void Initialize(Piece piece);
}