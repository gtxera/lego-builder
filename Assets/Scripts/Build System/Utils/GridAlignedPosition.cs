using System;
using UnityEngine;

public class GridAlignedPosition : MonoBehaviour
{
    [SerializeField]
    private PieceVector _position;

    [SerializeField]
    private bool _offsetByHalf;

    private void OnValidate()
    {
        transform.localPosition = _position.ToWorld() + (_offsetByHalf ? new PieceVector(1, 1).ToWorld() / 2f : Vector3.zero);
    }
}
