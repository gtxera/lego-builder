using System;
using KBCore.Refs;
using UnityEngine;

public class PieceParticleEmitter : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private ParticleSystem _particleSystem;

    private EventBinding<PieceCreatedEvent> _onPieceCreated;
    private EventBinding<PieceMovedEvent> _onPieceMoved;
    
    private void Awake()
    {
        _onPieceCreated = new EventBinding<PieceCreatedEvent>(OnPieceCreated);
        _onPieceMoved = new EventBinding<PieceMovedEvent>(OnPieceMoved);
        
        EventBus<PieceCreatedEvent>.Register(_onPieceCreated);
        EventBus<PieceMovedEvent>.Register(_onPieceMoved);
    }

    private void OnDestroy()
    {
        EventBus<PieceCreatedEvent>.Deregister(_onPieceCreated);
        EventBus<PieceMovedEvent>.Deregister(_onPieceMoved);
    }

    private void OnPieceMoved(PieceMovedEvent pieceMovedEvent)
    {
        Emit(pieceMovedEvent.CurrentPosition, pieceMovedEvent.Piece.Template.GetSize());
    }

    private void OnPieceCreated(PieceCreatedEvent pieceCreatedEvent)
    {
        Emit(pieceCreatedEvent.Piece.transform.position, pieceCreatedEvent.Piece.Template.GetSize());   
    }

    private void Emit(Vector3 position, PieceVector size)
    {
        var shape = _particleSystem.shape;
        shape.scale = size.ToWorld();
        transform.position = position;
        _particleSystem.Emit(5);
    }
}
