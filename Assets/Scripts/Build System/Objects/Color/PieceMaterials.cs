using UnityEngine;
public class PieceMaterials
{
    private Material _baseMaterial;
    private Material _transparentMaterial;

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

    public Material GetMaterial(bool transparent)
    {
        return transparent ? TransparentMaterial : BaseMaterial;
    }
}
