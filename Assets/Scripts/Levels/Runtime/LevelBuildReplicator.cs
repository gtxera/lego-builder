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
        
        if (_progressManager.IsCompleted(_replicatedBuildLevel))
            ReplicateLevelBuildNoAnimation(_progressManager.GetBuildData(_replicatedBuildLevel));
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
            var targetLocalPosition = pieceData.TransientData.LocalPosition;
            var startLocalPosition = targetLocalPosition + Vector3.up * 20f;
            sequence
                .Group(
                    Sequence.Create(Tween.Delay(delay,
                        () =>
                {
                    var piece = new GameObject("Piece").AddComponent<Piece>();
                    piece.transform.SetParent(transform, false);
                    piece.Initialize(pieceData, localSpace: true);
                    pieceHolder.Piece = piece;
                    piece.transform.localPosition = startLocalPosition;
                }))
                        .Chain(Tween.Custom(startLocalPosition, targetLocalPosition, 1f, value =>
                        {
                            pieceHolder.Piece.transform.localPosition = value;
                        })));
            delay += 0.05f;
            Debug.Log(delay);
        }
    }

    private void ReplicateLevelBuildNoAnimation(BuildData data)
    {
        Debug.Log("aqui");
        foreach (var pieceData in data.Pieces)
        {
            var piece = new GameObject("Piece").AddComponent<Piece>();
            piece.transform.SetParent(transform, false);
            piece.Initialize(pieceData, localSpace: true);
            piece.transform.localPosition = pieceData.TransientData.LocalPosition;
        }
    }

    private class PieceHolder
    {
        public Piece Piece { get; set; } = null;
    }
}
