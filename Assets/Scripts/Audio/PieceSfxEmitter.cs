using System;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

public class PieceSfxEmitter : MonoBehaviour
{
    [FormerlySerializedAs("_sfxEvent")]
    [SerializeField]
    private FMODUnity.EventReference _placementSfxEvent;

    [SerializeField]
    private FMODUnity.EventReference _removalSfxEvent;

    [SerializeField]
    private FMODUnity.EventReference _buildCompletedEvent;

    private EventBinding<PieceCreatedEvent> _onPieceCreated;
    private EventBinding<PieceMovedEvent> _onPieceMoved;
    private EventBinding<PieceRemovedEvent> _onPieceRemoved;
    
    private void Awake()
    {
        _onPieceCreated = new EventBinding<PieceCreatedEvent>(OnPieceCreated);
        _onPieceMoved = new EventBinding<PieceMovedEvent>(OnPieceMoved);
        _onPieceRemoved = new EventBinding<PieceRemovedEvent>(OnPieceRemoved);
        
        EventBus<PieceCreatedEvent>.Register(_onPieceCreated);
        EventBus<PieceMovedEvent>.Register(_onPieceMoved);
        EventBus<PieceRemovedEvent>.Register(_onPieceRemoved);
    }

    private void OnPieceCreated(PieceCreatedEvent pieceCreatedEvent) => Play(pieceCreatedEvent.Piece.transform.position);

    private void OnPieceMoved(PieceMovedEvent pieceMovedEvent) => Play(pieceMovedEvent.CurrentPosition);
    
    private void OnPieceRemoved(PieceRemovedEvent pieceRemovedEvent) => Play(pieceRemovedEvent.Piece.transform.position);
    
    private void Play(Vector3 position)
    {
        FMODUnity.RuntimeManager.PlayOneShot(_placementSfxEvent, position);
    }
}
