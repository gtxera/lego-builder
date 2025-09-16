using UnityEngine;

[CreateAssetMenu(fileName = "PieceTag", menuName = "Scriptable Objects/PieceTag")]
public class PieceTag : ScriptableObject
{
    [SerializeField]
    private string _singularName;

    [SerializeField]
    private string _pluralName;

    public string GetName(int count) => count > 1 ? _pluralName : _singularName;
}
