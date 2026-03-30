using UnityEngine;
public class PieceMaterials
{
    private Material _baseMaterial;
    private Material _transparentMaterial;
    private Material _ghostMaterial;

    public Material BaseMaterial
    {
        get
        {
            if (_baseMaterial == null)
                _baseMaterial = Resources.Load<Material>("Materials/Piece/DefaultPieceMaterial");
            
            return _baseMaterial;
        }
    }

    public Material TransparentMaterial
    {
        get
        {
            if (_transparentMaterial == null)
                _transparentMaterial = Resources.Load<Material>("Materials/Piece/TranslucentPieceMaterial");
            
            return _transparentMaterial;
        }
    }

    public Material GhostMaterial
    {
        get
        {
            if (_ghostMaterial == null)
                _ghostMaterial = Resources.Load<Material>("Materials/Piece/GhostPieceMaterial");

            return _ghostMaterial;
        }
    }

    public Material GetMaterial(bool transparent)
    {
        return transparent ? TransparentMaterial : BaseMaterial;
    }
}
