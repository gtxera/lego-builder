using System.Collections.Generic;
using UnityEngine;

public interface IPieceTemplate
{
    void Configure(GameObject pieceObject);

    PieceVector GetSize();

    int GetColorCount();

    IEnumerable<Vector3> GetSocketPositions();

    IEnumerable<Vector3> GetStudPositions();
}
