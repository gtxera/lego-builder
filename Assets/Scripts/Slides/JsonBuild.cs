using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "JsonBuild", menuName = "Scriptable Objects/JsonBuild")]
public class JsonBuild : ScriptableObject
{
    [SerializeField]
    private string _json;

    public BuildData GetBuildData() => JsonUtility.FromJson<BuildData>(_json);

    public static void Create(BuildData buildData)
    {
        #if UNITY_EDITOR
        var jsonBuild = CreateInstance<JsonBuild>();
        jsonBuild._json = JsonUtility.ToJson(buildData);
        jsonBuild.name = Guid.NewGuid().ToString();
        AssetDatabase.CreateAsset(jsonBuild, $"Assets/Builds/{jsonBuild.name}.asset");
        #endif
    }
}
