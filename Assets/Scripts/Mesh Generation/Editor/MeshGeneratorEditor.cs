using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshGeneratorEditor : EditorWindow
{
    [SerializeField]
    private PieceVector _pieceVector;

    [SerializeField]
    private int a;
    
    [MenuItem("Tools/Mesh Generator")]
    public static void ShowWindow()
    {
        GetWindow<MeshGeneratorEditor>();
    }
    
    private void CreateGUI()
    {
        var obj = new SerializedObject(this);
        var vecProperty = obj.FindProperty(nameof(_pieceVector));
        var button = new Button();
        button.clicked += () =>
        {
            new MeshGenerator().GetPieceMesh(new PieceVector(3, 2, .96f), PieceMeshType.WithStuds);
        };
        
        rootVisualElement.Add(button);
    }
}
