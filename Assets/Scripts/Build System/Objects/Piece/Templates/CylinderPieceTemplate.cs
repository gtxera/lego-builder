using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Extensions;
using UnityEngine;

[Serializable]
public class CylinderPieceTemplate : IPieceTemplate
{
    [SerializeField]
    private int _radius = 1;

    private const float Height = .96f;

    public CylinderPieceTemplate(int radius)
    {
        _radius = Mathf.Max(1, radius);
    }

    public void Configure(GameObject pieceObject)
    {
        var size = GetSize();
        var piecePartsPool = pieceObject.scene.GetSceneContainer().Resolve<PiecePartsPool>();

        var body = piecePartsPool.GetCylinder(size);
        body.transform.SetParent(pieceObject.transform, false);

        var collider = pieceObject.AddComponent<BoxCollider>();
        collider.size = size.ToWorld();

        foreach (var studPosition in GetStudPositions())
        {
            var stud = piecePartsPool.GetStud();
            stud.transform.SetParent(pieceObject.transform, false);
            stud.transform.localPosition = studPosition;
        }
    }

    public void OnDestroy(GameObject pieceObject)
    {
        var piecePartsPool = pieceObject.scene.GetSceneContainer().Resolve<PiecePartsPool>();

        piecePartsPool.ReturnCylinder(pieceObject.GetComponentInChildren<CylinderMarker>());

        foreach (var stud in pieceObject.GetComponentsInChildren<StudMarker>())
            piecePartsPool.ReturnStud(stud);
    }

    public PieceVector GetSize()
    {
        var clampedRadius = Mathf.Max(1, _radius);
        return new PieceVector(clampedRadius, clampedRadius, Height);
    }

    public int GetColorCount() => 1;

    public IEnumerable<Vector3> GetSocketPositions()
    {
        return GetConnectorPositions(-GetSize().ToWorld().y / 2f);
    }

    public IEnumerable<Vector3> GetStudPositions()
    {
        return GetConnectorPositions(GetSize().ToWorld().y / 2f);
    }

    public IEnumerable<PieceTag> GetTags() => Enumerable.Empty<PieceTag>();

    private IEnumerable<Vector3> GetConnectorPositions(float height)
    {
        var size = GetSize();
        var offset = new Vector3((size.X - 1) * .4f, 0f, (size.Y - 1) * .4f);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++)
            yield return new PieceVector(x, y, height).ToWorld() - offset;
    }
}
