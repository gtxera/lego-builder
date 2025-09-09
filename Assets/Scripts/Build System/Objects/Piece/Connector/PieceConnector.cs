using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class PieceConnector<TConnector, TConnecting> : PieceConnector 
    where TConnector : PieceConnector<TConnector, TConnecting>
    where TConnecting : PieceConnector<TConnecting, TConnector>
{
    private Guid _ownerPieceId;

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
        _ownerPieceId = piece.Id;
    }

    public sealed override bool IsOwnedBy(Piece piece) => _ownerPieceId == piece.Id;
    
    private void Connect(TConnecting connecting)
    {
        if (Connected || _ownerPieceId == connecting._ownerPieceId)
            return;
        
        _connecting = connecting;
        Connected = true;
        _connecting.Connect((TConnector)this);
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