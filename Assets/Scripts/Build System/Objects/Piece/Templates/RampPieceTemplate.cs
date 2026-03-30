using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Extensions;
using UnityEngine;

[Serializable]
public class RampPieceTemplate : IPieceTemplate
{
    [SerializeField]
    private int _width = 2;
    [SerializeField]
    private int _brickLength = 2;

    [SerializeField]
    private int _rampLength = 1;

    [SerializeField]
    private bool _inverted;
    
    private PieceVector BodySize => new(_width, _brickLength, Height);
    private PieceVector RampSize => new(_width, _rampLength, Height);
    
    private const float Height = .96f;
    
    public RampPieceTemplate(int width, int brickLength, int  rampLength, bool inverted)
    {
        _width = width;
        _brickLength = brickLength;
        _rampLength = rampLength;
        _inverted = inverted;
    }
    
    public void Configure(GameObject pieceObject)
    {
        var piecePartsPool = pieceObject.scene.GetSceneContainer().Resolve<PiecePartsPool>();

        var totalLength = Conversions.ToWorld(_brickLength + _rampLength);
        var halfBodyLength = Conversions.ToWorld(_brickLength) / 2f;
        var halfRampLength = Conversions.ToWorld(_rampLength) / 2f;
        var bodyPosition = new Vector3(0f, 0f, halfBodyLength - totalLength / 2f + 0.01f);
        var rampPosition = new Vector3(0f, 0f, totalLength / 2f - halfRampLength - 0.01f);

        var body = piecePartsPool.GetBody(BodySize);
        body.AddComponent<MeshCollider>().convex = true;
        body.transform.parent = pieceObject.transform;
        body.transform.localPosition = bodyPosition;

        var ramp = piecePartsPool.GetRamp(RampSize);
        ramp.AddComponent<MeshCollider>().convex = true;
        ramp.transform.parent = pieceObject.transform;
        ramp.transform.localPosition = rampPosition;
        ramp.transform.localRotation = Quaternion.Euler(0, 0, _inverted ? 180 : 0);

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
        
        piecePartsPool.ReturnBody(pieceObject.GetComponentInChildren<BodyMarker>());
        piecePartsPool.ReturnRamp(pieceObject.GetComponentInChildren<RampMarker>());

        foreach (var stud in pieceObject.GetComponentsInChildren<StudMarker>())
            piecePartsPool.ReturnStud(stud);
    }

    public PieceVector GetSize() => new(_width, _brickLength + _rampLength, Height);

    public int GetColorCount() => 1;
    
    public IEnumerable<Vector3> GetSocketPositions()
    {
        var halfHeight = BodySize.ToWorld().y / 2;
        return _inverted ? GetTopPositions(-halfHeight) : GetBottomPositions(-halfHeight);
    }

    public IEnumerable<Vector3> GetStudPositions()
    {
        var halfHeight = BodySize.ToWorld().y / 2;
        return _inverted ? GetBottomPositions(halfHeight) : GetTopPositions(halfHeight);
    }

    private IEnumerable<Vector3> GetBottomPositions(float height)
    {
        var offset = new Vector3((_width - 1) * .4f, 0, (_brickLength + _rampLength - 1) * .4f);

        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _brickLength + _rampLength; y++) 
            yield return new PieceVector(x, y, height).ToWorld() - offset;
    }

    private IEnumerable<Vector3> GetTopPositions(float height)
    {
        var rampBodyOffset = new PieceVector(0, _rampLength - _brickLength).ToWorld() / 2;
        var offset = new Vector3((_width - 1) * .4f, 0, (_brickLength - 1) * .4f) + rampBodyOffset + new PieceVector(0, _brickLength).ToWorld() / 2;

        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _brickLength; y++) 
            yield return new PieceVector(x, y, height).ToWorld() - offset;
    }

    public IEnumerable<PieceTag> GetTags() => Enumerable.Empty<PieceTag>();

    public bool IsSymmetricOnXAxis() => true;

    public bool IsSymmetricOnYAxis() => false;

    public bool IsSymmetricOnAllAxes() => false;
}
