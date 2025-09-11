using System;
using UnityEngine;

public class GridAlignedPosition : MonoBehaviour
{
    [SerializeField]
    private PieceVector _position;

    private void OnValidate()
    {
        transform.position = _position.ToWorld();
    }
}
