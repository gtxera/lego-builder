using UnityEngine;

public static class PieceCreationHelper
{
    private static Material _defaultPieceMaterial;
    
    public static GameObject MakeBody(PieceVector size)
    {
        var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brick.transform.localScale = size.ToWorld() - new Vector3(0.02f, 0, 0.02f);
        Object.Destroy(brick.GetComponent<BoxCollider>());
        brick.GetComponent<Renderer>().sharedMaterial = GetPieceMaterial();

        return brick;
    }

    public static GameObject MakeStud(Vector3 position)
    {
        var stud = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stud.transform.localScale = new Vector3(0.48f, 0.18f, 0.48f);
        Object.Destroy(stud.GetComponent<CapsuleCollider>());
        stud.GetComponent<Renderer>().sharedMaterial = GetPieceMaterial();

        return stud;
    }

    private static Material GetPieceMaterial()
    {
        if (_defaultPieceMaterial == null)
            _defaultPieceMaterial = Resources.Load<Material>("Materials/Piece/DefaultPieceMaterial");

        return _defaultPieceMaterial;
    }
}
