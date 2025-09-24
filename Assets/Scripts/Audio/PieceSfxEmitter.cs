using System;
using UnityEngine;

public class PieceSfxEmitter : MonoBehaviour
{
    [SerializeField]
    private FMODUnity.EventReference _sfxEvent;

    private EventBinding<PieceCreatedEvent> _onPieceCreated;
    private EventBinding<PieceMovedEvent> _onPieceMoved;
    
    private void Awake()
    {
        _onPieceCreated = new EventBinding<PieceCreatedEvent>(OnPieceCreated);
        _onPieceMoved = new EventBinding<PieceMovedEvent>(OnPieceMoved);
        
        EventBus<PieceCreatedEvent>.Register(_onPieceCreated);
        EventBus<PieceMovedEvent>.Register(_onPieceMoved);
    }

    private void OnPieceCreated(PieceCreatedEvent pieceCreatedEvent) => Play(pieceCreatedEvent.Piece.transform.position);

    private void OnPieceMoved(PieceMovedEvent pieceMovedEvent) => Play(pieceMovedEvent.CurrentPosition);
    
    private void Play(Vector3 position)
    {
        FMODUnity.RuntimeManager.PlayOneShot(_sfxEvent, position);
    }
}
