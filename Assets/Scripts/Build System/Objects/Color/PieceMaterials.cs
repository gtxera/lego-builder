using UnityEngine;

public class PieceMaterials
{
    private const string DefaultPieceMaterialPath = "Materials/Piece/DefaultPieceMaterial";
    private const string TranslucentPieceMaterialPath = "Materials/Piece/TranslucentPieceMaterial";
    private const string GhostPieceMaterialPath = "Materials/Piece/GhostPieceMaterial";
    private const string SoftTransparentShaderName = "LegoBuilder/Soft Transparent Piece";

    private Material _baseMaterial;
    private Material _transparentMaterial;
    private Material _ghostMaterial;

    public Material BaseMaterial
    {
        get
        {
            if (_baseMaterial == null)
                _baseMaterial = Resources.Load<Material>(DefaultPieceMaterialPath);

            return _baseMaterial;
        }
    }

    public Material TransparentMaterial
    {
        get
        {
            if (_transparentMaterial == null)
            {
                var sourceMaterial = Resources.Load<Material>(TranslucentPieceMaterialPath);
                if (sourceMaterial == null)
                    return null;

                _transparentMaterial = new Material(sourceMaterial)
                {
                    name = sourceMaterial.name
                };

                var softTransparentShader = Shader.Find(SoftTransparentShaderName);
                if (softTransparentShader != null)
                    _transparentMaterial.shader = softTransparentShader;
            }

            return _transparentMaterial;
        }
    }

    public Material GhostMaterial
    {
        get
        {
            if (_ghostMaterial == null)
                _ghostMaterial = Resources.Load<Material>(GhostPieceMaterialPath);

            return _ghostMaterial;
        }
    }

    public Material GetMaterial(bool transparent)
    {
        return transparent ? TransparentMaterial : BaseMaterial;
    }
}
