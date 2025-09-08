using System.Collections.Generic;
using UnityEngine;

public interface IPieceTemplate
{
    void Configure(GameObject pieceObject);

    Vector3 GetSize();

    int GetColorCount();

    IEnumerable<Vector3> GetSocketPositions();

    IEnumerable<Vector3> GetStudPositions();
}
