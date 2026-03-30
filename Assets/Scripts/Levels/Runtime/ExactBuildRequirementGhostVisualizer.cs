using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class ExactBuildRequirementGhostVisualizer : IDisposable
{
    private const float GhostAlpha = 0.18f;
    private const float BoundsTolerance = 0.0001f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int SourceTransparentId = Shader.PropertyToID("_SourceTransparent");
    private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
    private static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");
    private static readonly int GlowWidthId = Shader.PropertyToID("_GlowWidth");

    private readonly ExactBuildRequirement _requirement;
    private readonly Transform _parent;
    private readonly MaterialPropertyBlock _materialPropertyBlock = new();

    private GameObject _ghostRoot;
    private BuildData _requiredBuildData;
    private GhostPieceEntry[] _ghostPieces = Array.Empty<GhostPieceEntry>();
    private List<int>[] _connections = Array.Empty<List<int>>();
    private int[] _basePieceIndices = Array.Empty<int>();

    public ExactBuildRequirementGhostVisualizer(ExactBuildRequirement requirement, Transform parent)
    {
        _requirement = requirement;
        _parent = parent;
    }

    public void Show(Build currentBuild)
    {
        Hide();

        _requiredBuildData = _requirement.GetRequiredBuildData();
        if (_requiredBuildData == null)
            return;

        _ghostRoot = new GameObject("Exact Build Ghost");
        _ghostRoot.transform.SetParent(_parent, false);

        InitializeGhostPieces();
        Refresh(currentBuild);
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

    public void Refresh(Build currentBuild)
    {
        if (_ghostRoot == null || _requiredBuildData == null)
            return;

        var matchResult = ExactBuildRequirementMatcher.GetMatchResult(_requiredBuildData, currentBuild?.GetBuildData());
        var visibleIndices = DetermineVisiblePieceIndices(matchResult);

        for (var i = 0; i < _ghostPieces.Length; i++)
            _ghostPieces[i].GameObject.SetActive(visibleIndices.Contains(i));
    }

    private void InitializeGhostPieces()
    {
        var requiredPieces = (_requiredBuildData.Pieces ?? Array.Empty<PieceData>()).ToArray();
        _ghostPieces = new GhostPieceEntry[requiredPieces.Length];

        for (var i = 0; i < requiredPieces.Length; i++)
        {
            var pieceData = requiredPieces[i];
            var ghostPieceObject = CreateGhostPiece(pieceData);
            var bounds = GetLocalBounds(pieceData);
            _ghostPieces[i] = new GhostPieceEntry(pieceData, ghostPieceObject, bounds.min.y);
        }

        _connections = BuildConnections();

        var lowestBaseHeight = _ghostPieces.Length == 0
            ? 0f
            : _ghostPieces.Min(piece => piece.BaseHeight);

        _basePieceIndices = _ghostPieces
            .Select((piece, index) => new { piece.BaseHeight, Index = index })
            .Where(entry => Mathf.Abs(entry.BaseHeight - lowestBaseHeight) <= BoundsTolerance)
            .Select(entry => entry.Index)
            .ToArray();
    }

    private HashSet<int> DetermineVisiblePieceIndices(ExactBuildRequirementMatcher.MatchResult matchResult)
    {
        var targetIndices = GetNextIncompleteHeightIndices(matchResult);
        if (targetIndices.Count == 0)
            return targetIndices;

        foreach (var index in targetIndices.ToArray())
            IncludeSupportChain(index, targetIndices);

        return targetIndices;
    }

    private HashSet<int> GetNextIncompleteHeightIndices(ExactBuildRequirementMatcher.MatchResult matchResult)
    {
        var lowestIncompleteHeight = float.PositiveInfinity;

        for (var i = 0; i < _ghostPieces.Length; i++)
        {
            if (matchResult.IsRequiredPieceMatched(i))
                continue;

            lowestIncompleteHeight = Mathf.Min(lowestIncompleteHeight, _ghostPieces[i].BaseHeight);
        }

        if (float.IsPositiveInfinity(lowestIncompleteHeight))
            return new HashSet<int>();

        var visibleIndices = new HashSet<int>();
        for (var i = 0; i < _ghostPieces.Length; i++)
        {
            if (matchResult.IsRequiredPieceMatched(i))
                continue;

            if (Mathf.Abs(_ghostPieces[i].BaseHeight - lowestIncompleteHeight) <= BoundsTolerance)
                visibleIndices.Add(i);
        }

        return visibleIndices;
    }

    private void IncludeSupportChain(int startIndex, HashSet<int> visibleIndices)
    {
        if (visibleIndices.Contains(startIndex) && HasVisiblePathToBase(startIndex, visibleIndices))
            return;

        var pathToBase = FindPathToBase(startIndex);
        if (pathToBase.Count == 0)
        {
            foreach (var connectedIndex in EnumerateConnectedComponent(startIndex))
                visibleIndices.Add(connectedIndex);

            return;
        }

        foreach (var pathIndex in pathToBase)
            visibleIndices.Add(pathIndex);
    }

    private bool HasVisiblePathToBase(int startIndex, HashSet<int> visibleIndices)
    {
        var queue = new Queue<int>();
        var visited = new HashSet<int>();
        queue.Enqueue(startIndex);
        visited.Add(startIndex);

        while (queue.Count > 0)
        {
            var index = queue.Dequeue();
            if (_basePieceIndices.Contains(index))
                return true;

            foreach (var connectedIndex in _connections[index])
            {
                if (!visibleIndices.Contains(connectedIndex) || !visited.Add(connectedIndex))
                    continue;

                queue.Enqueue(connectedIndex);
            }
        }

        return false;
    }

    private List<int> FindPathToBase(int startIndex)
    {
        if (_basePieceIndices.Contains(startIndex))
            return new List<int> { startIndex };

        var queue = new Queue<int>();
        var visited = new HashSet<int> { startIndex };
        var previous = new Dictionary<int, int>();
        queue.Enqueue(startIndex);

        while (queue.Count > 0)
        {
            var index = queue.Dequeue();

            foreach (var connectedIndex in _connections[index])
            {
                if (!visited.Add(connectedIndex))
                    continue;

                previous[connectedIndex] = index;
                if (_basePieceIndices.Contains(connectedIndex))
                    return ReconstructPath(startIndex, connectedIndex, previous);

                queue.Enqueue(connectedIndex);
            }
        }

        return new List<int>();
    }

    private IEnumerable<int> EnumerateConnectedComponent(int startIndex)
    {
        var queue = new Queue<int>();
        var visited = new HashSet<int> { startIndex };
        queue.Enqueue(startIndex);

        while (queue.Count > 0)
        {
            var index = queue.Dequeue();
            yield return index;

            foreach (var connectedIndex in _connections[index])
            {
                if (visited.Add(connectedIndex))
                    queue.Enqueue(connectedIndex);
            }
        }
    }

    private static List<int> ReconstructPath(int startIndex, int endIndex, IReadOnlyDictionary<int, int> previous)
    {
        var path = new List<int>();
        var currentIndex = endIndex;

        path.Add(currentIndex);
        while (currentIndex != startIndex && previous.TryGetValue(currentIndex, out var previousIndex))
        {
            currentIndex = previousIndex;
            path.Add(currentIndex);
        }

        return path;
    }

    private List<int>[] BuildConnections()
    {
        var connections = new List<int>[_ghostPieces.Length];
        for (var i = 0; i < connections.Length; i++)
            connections[i] = new List<int>();

        for (var i = 0; i < _ghostPieces.Length; i++)
        {
            var firstBounds = GetLocalBounds(_ghostPieces[i].PieceData);

            for (var j = i + 1; j < _ghostPieces.Length; j++)
            {
                var secondBounds = GetLocalBounds(_ghostPieces[j].PieceData);
                if (!AreConnected(firstBounds, secondBounds))
                    continue;

                connections[i].Add(j);
                connections[j].Add(i);
            }
        }

        return connections;
    }

    private GameObject CreateGhostPiece(PieceData pieceData)
    {
        var ghostPieceObject = new GameObject("Ghost Piece");
        ghostPieceObject.transform.SetParent(_ghostRoot.transform, false);

        var sourceWasTransparent = IsOriginallyTransparent(pieceData);
        var piece = ghostPieceObject.AddComponent<Piece>();
        piece.Initialize(CreateGhostPieceData(pieceData), localSpace: true);
        ApplyGhostVisuals(ghostPieceObject, sourceWasTransparent);
        DisableInteraction(ghostPieceObject);
        return ghostPieceObject;
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

    private bool IsOriginallyTransparent(PieceData pieceData)
    {
        return pieceData.TransientData.Colors?.Any(color => color is { Transparent: true }) == true;
    }

    private void ApplyGhostVisuals(GameObject ghostPieceObject, bool sourceWasTransparent)
    {
        var pieceMaterials = ghostPieceObject.scene.GetSceneContainer().Resolve<PieceMaterials>();
        var ghostMaterial = pieceMaterials.GhostMaterial;
        if (ghostMaterial == null)
            return;

        foreach (var renderer in ghostPieceObject.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sharedMaterial = ghostMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            _materialPropertyBlock.Clear();
            var baseColor = Color.white;
            renderer.GetPropertyBlock(_materialPropertyBlock);
            if (_materialPropertyBlock.HasColor(BaseColorId))
                baseColor = _materialPropertyBlock.GetColor(BaseColorId);

            _materialPropertyBlock.SetColor(BaseColorId, baseColor);
            _materialPropertyBlock.SetFloat(AlphaId, GhostAlpha);
            _materialPropertyBlock.SetFloat(SourceTransparentId, sourceWasTransparent ? 1f : 0f);
            _materialPropertyBlock.SetColor(GlowColorId, Color.white);
            _materialPropertyBlock.SetFloat(GlowIntensityId, sourceWasTransparent ? 1.35f : 0f);
            _materialPropertyBlock.SetFloat(GlowWidthId, 3.2f);
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
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

    private static Bounds GetLocalBounds(PieceData pieceData)
    {
        var size = pieceData.Template.GetSize().ToWorld();
        if (pieceData.TransientData.Rotation is PieceRotation.East or PieceRotation.West)
            (size.x, size.z) = (size.z, size.x);

        return new Bounds(pieceData.TransientData.LocalPosition, size);
    }

    private static bool AreConnected(Bounds firstBounds, Bounds secondBounds)
    {
        var overlapX = GetAxisOverlap(firstBounds.min.x, firstBounds.max.x, secondBounds.min.x, secondBounds.max.x);
        var overlapY = GetAxisOverlap(firstBounds.min.y, firstBounds.max.y, secondBounds.min.y, secondBounds.max.y);
        var overlapZ = GetAxisOverlap(firstBounds.min.z, firstBounds.max.z, secondBounds.min.z, secondBounds.max.z);

        if (overlapX < -BoundsTolerance || overlapY < -BoundsTolerance || overlapZ < -BoundsTolerance)
            return false;

        var touchingAxes = 0;
        if (Mathf.Abs(overlapX) <= BoundsTolerance)
            touchingAxes++;
        if (Mathf.Abs(overlapY) <= BoundsTolerance)
            touchingAxes++;
        if (Mathf.Abs(overlapZ) <= BoundsTolerance)
            touchingAxes++;

        var overlappingAxes = 0;
        if (overlapX > BoundsTolerance)
            overlappingAxes++;
        if (overlapY > BoundsTolerance)
            overlappingAxes++;
        if (overlapZ > BoundsTolerance)
            overlappingAxes++;

        return touchingAxes >= 1 && overlappingAxes >= 2;
    }

    private static float GetAxisOverlap(float firstMin, float firstMax, float secondMin, float secondMax)
    {
        return Mathf.Min(firstMax, secondMax) - Mathf.Max(firstMin, secondMin);
    }

    private readonly struct GhostPieceEntry
    {
        public GhostPieceEntry(PieceData pieceData, GameObject gameObject, float baseHeight)
        {
            PieceData = pieceData;
            GameObject = gameObject;
            BaseHeight = baseHeight;
        }

        public PieceData PieceData { get; }
        public GameObject GameObject { get; }
        public float BaseHeight { get; }
    }
}
