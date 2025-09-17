using System.Linq;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class LevelBuildReplicator : MonoBehaviour
{
    [Inject]
    private readonly ProgressManager _progressManager;

    [SerializeField]
    private Level _replicatedBuildLevel;
    
    private void Awake()
    {
        _progressManager.SubscribeOnLevelCompleted(_replicatedBuildLevel, ReplicateLevelBuild);
    }

    private void ReplicateLevelBuild(BuildData data)
    {
        var sequence = Sequence.Create();
        
        foreach (var child in transform.Children().ToArray())
        {
            Destroy(child.gameObject);
        }

        var delay = .2f;
        foreach (var pieceData in data.Pieces)
        {
            var pieceHolder = new PieceHolder();
            sequence
                .Group(
                    Sequence.Create(Tween.Delay(delay,
                        () =>
                {
                    var piece = new GameObject("Piece").AddComponent<Piece>();
                    piece.transform.SetParent(transform, false);
                    piece.Initialize(pieceData);
                    pieceHolder.Piece = piece;
                    piece.transform.localPosition = Vector3.up * 20;
                }))
                        .Chain(Tween.Custom(Vector3.up * 20, pieceData.TransientData.LocalPosition, 1f, value =>
                        {
                            pieceHolder.Piece.transform.localPosition = value;
                        })));
            delay += 0.05f;
            Debug.Log(delay);
        }
    }

    private class PieceHolder
    {
        public Piece Piece { get; set; } = null;
    }
}
