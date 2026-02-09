using System.Linq;
using PrimeTween;
using UnityEngine;

public class SlideBuild : MonoBehaviour
{
    [SerializeField]
    private JsonBuild _build;

    [SerializeField]
    private Transform _piecesRoot;

    private BuildData _data;
    
    private void Awake()
    {
        _data = _build.GetBuildData();
    }

    public void ShowBuild()
    {
        HideBuild();
        
        var sequence = Sequence.Create();

        var delay = .2f;
        foreach (var pieceData in _data.Pieces)
        {
            var pieceHolder = new PieceHolder();
            sequence
                .Group(
                    Sequence.Create(Tween.Delay(delay,
                            () =>
                            {
                                var piece = new GameObject("Piece").AddComponent<Piece>();
                                piece.transform.SetParent(_piecesRoot, false);
                                piece.Initialize(pieceData);
                                pieceHolder.Piece = piece;
                                piece.transform.localPosition = Vector3.up * 20;
                            }))
                        .Chain(Tween.Custom(Vector3.up * 20, pieceData.TransientData.LocalPosition, 1f, value =>
                        {
                            pieceHolder.Piece.transform.localPosition = value;
                        })));
            delay += 0.015f;
        }
    }

    public void HideBuild()
    {
        foreach (var child in _piecesRoot.Children().ToArray())
        {
            Destroy(child.gameObject);
        }
    }
    
    private class PieceHolder
    {
        public Piece Piece { get; set; } = null;
    }
}
