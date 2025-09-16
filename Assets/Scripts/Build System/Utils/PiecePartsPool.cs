using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class PiecePartsPool : IDisposable
{
    private readonly ObjectPool<GameObject> _bodiesPool;
    private readonly ObjectPool<GameObject> _studsPool;

    private readonly Material _pieceMaterial;
    
    public PiecePartsPool()
    {
        _bodiesPool = new ObjectPool<GameObject>(CreateBody,
            gameObject =>
            {
                gameObject.SetActive(true);
                gameObject.transform.position = Vector3.zero;
            },
            gameObject =>
            {
                gameObject.transform.SetParent(null);
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.SetActive(false);
            });

        _studsPool = new ObjectPool<GameObject>(CreateStud,
            gameObject => gameObject.SetActive(true),
            gameObject =>
            {
                gameObject.transform.SetParent(null);
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.SetActive(false);
            });

        _pieceMaterial = Resources.Load<Material>("Materials/Piece/DefaultPieceMaterial");
    }

    public GameObject GetBody(PieceVector size)
    {
        var body = _bodiesPool.Get();
        body.transform.localScale = size.ToWorld() - new Vector3(0.02f, 0, 0.02f);

        return body;
    }

    public GameObject GetStud() => _studsPool.Get();

    public void ReturnBody(BodyMarker body)
    {
        _bodiesPool.Release(body.gameObject);
    }

    public void ReturnStud(StudMarker stud)
    {
        _studsPool.Release(stud.gameObject);
    }
    
    public void Dispose()
    {
        _bodiesPool?.Dispose();
        _studsPool?.Dispose();
        Resources.UnloadAsset(_pieceMaterial);
    }

    private GameObject CreateBody()
    {
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(body.GetComponent<BoxCollider>());
        body.GetComponent<Renderer>().sharedMaterial = _pieceMaterial;
        body.AddComponent<BodyMarker>();
        body.AddComponent<PieceColoredPart>();
        
        return body;
    }
    
    private GameObject CreateStud()
    {
        var stud = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stud.transform.localScale = new Vector3(0.48f, 0.18f, 0.48f);
        Object.Destroy(stud.GetComponent<CapsuleCollider>());
        stud.GetComponent<Renderer>().sharedMaterial = _pieceMaterial;
        stud.AddComponent<StudMarker>();
        stud.AddComponent<PieceColoredPart>();
        
        return stud;
    }
}
