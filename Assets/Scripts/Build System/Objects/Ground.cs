using System;
using UnityEngine;

public class Ground : MonoBehaviour
{
    [SerializeField]
    private int _width;

    [SerializeField]
    private int _length;

    private void Awake()
    {
        var offset = new Vector3((_width - 1) * .4f, 0, (_length - 1) * .4f);

        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _length; y++)
        {
            var localPosition = new PieceVector(x, y).ToWorld() - offset;
            var studGameObject = new GameObject("Stud");
            studGameObject.AddComponent<Stud>();
            studGameObject.transform.SetParent(transform);
            studGameObject.transform.localPosition = localPosition;
        }
    }
}
