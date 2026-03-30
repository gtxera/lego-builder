using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ExactBuildRequirementMatcher
{
    private const float PositionTolerance = 0.0001f;

    public static bool AreEquivalent(BuildData requiredBuild, BuildData actualBuild)
    {
        return GetMatchResult(requiredBuild, actualBuild).IsEquivalent;
    }

    public static MatchResult GetMatchResult(BuildData requiredBuild, BuildData actualBuild)
    {
        var requiredPieces = Enumerate(requiredBuild).Select(CreateSignature).ToList();
        var actualPieces = Enumerate(actualBuild).Select(CreateSignature).ToList();

        var matchedRequiredPieces = new bool[requiredPieces.Count];
        var matchedRequiredPieceCount = 0;
        var unmatchedActualPieceCount = 0;

        foreach (var actualPiece in actualPieces)
        {
            var matchIndex = FindMatch(requiredPieces, matchedRequiredPieces, actualPiece);
            if (matchIndex < 0)
            {
                unmatchedActualPieceCount++;
                continue;
            }

            matchedRequiredPieces[matchIndex] = true;
            matchedRequiredPieceCount++;
        }

        return new MatchResult(matchedRequiredPieces, matchedRequiredPieceCount, unmatchedActualPieceCount);
    }

    private static IEnumerable<PieceData> Enumerate(BuildData buildData)
    {
        return buildData?.Pieces ?? Array.Empty<PieceData>();
    }

    private static PieceSignature CreateSignature(PieceData pieceData)
    {
        var transientData = pieceData.TransientData;
        var namedColors = transientData.Colors?.Select(GetNamedColor).ToArray() ?? Array.Empty<NamedColor>();

        return new PieceSignature(
            GetTemplateSignature(pieceData.Template),
            pieceData.Template,
            transientData.LocalPosition,
            transientData.Rotation,
            namedColors);
    }

    private static NamedColor GetNamedColor(PieceColor pieceColor)
    {
        return pieceColor?.NamedColor;
    }

    private static int FindMatch(
        IReadOnlyList<PieceSignature> requiredPieces,
        IReadOnlyList<bool> matchedRequiredPieces,
        PieceSignature actualPiece)
    {
        for (var i = 0; i < requiredPieces.Count; i++)
        {
            if (matchedRequiredPieces[i])
                continue;

            if (requiredPieces[i].Matches(actualPiece))
                return i;
        }

        return -1;
    }

    private static string GetTemplateSignature(IPieceTemplate template)
    {
        if (template == null)
            return string.Empty;

        return $"{template.GetType().AssemblyQualifiedName}|{JsonUtility.ToJson(template)}";
    }

    private static bool AreEquivalentRotations(IPieceTemplate template, PieceRotation requiredRotation, PieceRotation actualRotation)
    {
        if (requiredRotation == actualRotation)
            return true;

        if (template == null)
            return false;

        if (template.IsSymmetricOnAllAxes())
            return true;

        if (template.IsSymmetricOnXAxis() && BelongsToSamePair(requiredRotation, actualRotation, PieceRotation.East, PieceRotation.West))
            return true;

        if (template.IsSymmetricOnYAxis() && BelongsToSamePair(requiredRotation, actualRotation, PieceRotation.North, PieceRotation.South))
            return true;

        return false;
    }

    private static bool BelongsToSamePair(PieceRotation lhs, PieceRotation rhs, PieceRotation first, PieceRotation second)
    {
        return (lhs == first || lhs == second) && (rhs == first || rhs == second);
    }

    private readonly struct PieceSignature
    {
        private readonly string _templateSignature;
        private readonly IPieceTemplate _template;
        private readonly Vector3 _localPosition;
        private readonly PieceRotation _rotation;
        private readonly NamedColor[] _colors;

        public PieceSignature(string templateSignature, IPieceTemplate template, Vector3 localPosition, PieceRotation rotation, NamedColor[] colors)
        {
            _templateSignature = templateSignature;
            _template = template;
            _localPosition = localPosition;
            _rotation = rotation;
            _colors = colors;
        }

        public bool Matches(PieceSignature other)
        {
            return _templateSignature == other._templateSignature &&
                   (_localPosition - other._localPosition).sqrMagnitude <= PositionTolerance &&
                   AreEquivalentRotations(_template, _rotation, other._rotation) &&
                   ColorsMatch(other._colors);
        }

        private bool ColorsMatch(IReadOnlyList<NamedColor> otherColors)
        {
            if (_colors.Length != otherColors.Count)
                return false;

            for (var i = 0; i < _colors.Length; i++)
            {
                if (!Equals(_colors[i], otherColors[i]))
                    return false;
            }

            return true;
        }
    }

    public sealed class MatchResult
    {
        private readonly bool[] _matchedRequiredPieces;

        public MatchResult(bool[] matchedRequiredPieces, int matchedRequiredPieceCount, int unmatchedActualPieceCount)
        {
            _matchedRequiredPieces = matchedRequiredPieces ?? Array.Empty<bool>();
            MatchedRequiredPieceCount = matchedRequiredPieceCount;
            UnmatchedActualPieceCount = unmatchedActualPieceCount;
        }

        public int RequiredPieceCount => _matchedRequiredPieces.Length;
        public int MatchedRequiredPieceCount { get; }
        public int UnmatchedActualPieceCount { get; }
        public bool IsEquivalent => UnmatchedActualPieceCount == 0 && MatchedRequiredPieceCount == RequiredPieceCount;

        public bool IsRequiredPieceMatched(int index)
        {
            return index >= 0 &&
                   index < _matchedRequiredPieces.Length &&
                   _matchedRequiredPieces[index];
        }
    }
}
