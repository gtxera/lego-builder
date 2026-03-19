using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PieceSelectionOutline
{
    private static readonly int OutlineWidthPropertyId = Shader.PropertyToID("_OutlineWidth");

    private readonly List<GameObject> _outlineObjects = new();

    public PieceSelectionOutline(Piece piece, Material outlineMaterial)
    {
        foreach (var renderer in piece.GetComponentsInChildren<MeshRenderer>(true))
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                continue;

            var outlineObject = new GameObject($"{renderer.gameObject.name} Outline", typeof(MeshFilter), typeof(MeshRenderer));
            outlineObject.transform.SetParent(renderer.transform, false);

            var outlineFilter = outlineObject.GetComponent<MeshFilter>();
            outlineFilter.sharedMesh = meshFilter.sharedMesh;

            var outlineRenderer = outlineObject.GetComponent<MeshRenderer>();
            outlineRenderer.sharedMaterial = outlineMaterial;
            outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            outlineRenderer.lightProbeUsage = LightProbeUsage.Off;
            outlineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            outlineRenderer.allowOcclusionWhenDynamic = false;
            outlineRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

            var propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetFloat(OutlineWidthPropertyId, GetOutlineWidth(renderer));
            outlineRenderer.SetPropertyBlock(propertyBlock);

            outlineObject.SetActive(false);
            _outlineObjects.Add(outlineObject);
        }
    }

    public void SetVisible(bool visible)
    {
        foreach (var outlineObject in _outlineObjects)
        {
            if (outlineObject.activeSelf == visible)
                continue;

            outlineObject.SetActive(visible);
        }
    }

    private static float GetOutlineWidth(Renderer renderer)
    {
        var bounds = renderer.bounds;
        var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        return Mathf.Clamp(maxSize * 0.03f, 0.012f, 0.04f);
    }
}
