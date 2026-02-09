using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[Serializable]
public class MeshPieceTemplate : IPieceTemplate
{
    [SerializeField]
    private string _meshPieceName;

    [SerializeField]
    private string[] _pieceTagNames;

    private MeshPiece _meshPiece;
    private MeshPiece MeshPiece
    {
        get
        {
            if (_meshPiece == null)
            {
                var scene = SceneManager.GetActiveScene();
                var container = scene.GetSceneContainer();
                var loader = container.Resolve<PieceResourceLoader<MeshPiece>>();
                _meshPiece = loader.Get(_meshPieceName);
            }

            return _meshPiece;
        }
    }
    
    private PieceTag[] _pieceTags;
    private IEnumerable<PieceTag> PieceTags
    {
        get
        {
            if (_pieceTags == null)
            {
                var scene = SceneManager.GetActiveScene();
                var container = scene.GetSceneContainer();
                var loader = container.Resolve<PieceResourceLoader<PieceTag>>();
                _pieceTags = loader.Get(_pieceTagNames).ToArray();
            }

            return _pieceTags;
        }
    }
    
    public void Configure(GameObject pieceObject)
    {
        var resourceLoader = pieceObject.scene.GetSceneContainer().Resolve<PieceResourceLoader<MeshPiece>>();

        var prefab = resourceLoader.Get(_meshPieceName);
        
        Object.Instantiate(prefab, pieceObject.transform);
    }

    public void OnDestroy(GameObject pieceObject) { }

    public PieceVector GetSize() => MeshPiece.Size;

    public int GetColorCount() => 1;

    public IEnumerable<Vector3> GetSocketPositions()
    {
        var size = GetSize();
        var halfHeight = size.ToWorld().y / 2;
        var offset = new Vector3((size.X - 1) * .4f, 0, (size.Y - 1) * .4f);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++) 
            yield return new PieceVector(x, y, -halfHeight).ToWorld() - offset;
    }

    public IEnumerable<Vector3> GetStudPositions() => Enumerable.Empty<Vector3>();

    public IEnumerable<PieceTag> GetTags() => PieceTags;
}
