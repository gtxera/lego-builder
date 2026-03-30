using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class ExactBuildRequirementGhostVisualizer : IDisposable
{
    private readonly ExactBuildRequirement _requirement;
    private readonly Transform _parent;

    private GameObject _ghostRoot;

    public ExactBuildRequirementGhostVisualizer(ExactBuildRequirement requirement, Transform parent)
    {
        _requirement = requirement;
        _parent = parent;
    }

    public void Show()
    {
        Hide();

        var buildData = _requirement.GetRequiredBuildData();
        if (buildData == null)
            return;

        _ghostRoot = new GameObject("Exact Build Ghost");
        _ghostRoot.transform.SetParent(_parent, false);

        foreach (var pieceData in buildData.Pieces ?? Array.Empty<PieceData>())
            CreateGhostPiece(pieceData);
    }

    public void Hide()
    {
        if (_ghostRoot == null)
            return;

        Object.Destroy(_ghostRoot);
        _ghostRoot = null;
    }

    public void Dispose()
    {
        Hide();
    }

    private void CreateGhostPiece(PieceData pieceData)
    {
        var ghostPieceObject = new GameObject("Ghost Piece");
        ghostPieceObject.transform.SetParent(_ghostRoot.transform, false);

        var piece = ghostPieceObject.AddComponent<Piece>();
        piece.Initialize(CreateGhostPieceData(pieceData), localSpace: true);
        DisableInteraction(ghostPieceObject);
    }

    private static PieceData CreateGhostPieceData(PieceData pieceData)
    {
        var transientData = pieceData.TransientData;
        var ghostColors = transientData.Colors?.Select(CreateGhostColor).ToArray() ?? Array.Empty<PieceColor>();
        var ghostTransientData = new PieceTransientData(
            Guid.NewGuid(),
            transientData.LocalPosition,
            ghostColors,
            transientData.Rotation,
            transientData.CreationTime,
            transientData.WorldPosition);

        return new PieceData(pieceData.Template, ghostTransientData);
    }

    private static PieceColor CreateGhostColor(PieceColor sourceColor)
    {
        if (sourceColor == null)
            return new SimpleColor(Color.white, true);

        return new SimpleColor(sourceColor.Color, true);
    }

    private static void DisableInteraction(GameObject ghostPieceObject)
    {
        foreach (var collider in ghostPieceObject.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;

        foreach (var connector in ghostPieceObject.GetComponentsInChildren<PieceConnector>(true))
            connector.enabled = false;

        if (ghostPieceObject.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = false;
        }

        var outline = ghostPieceObject.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        SetLayerRecursively(ghostPieceObject.transform, LayerMask.NameToLayer("Ignore Raycast"));
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        foreach (Transform child in root)
            SetLayerRecursively(child, layer);
    }
}
