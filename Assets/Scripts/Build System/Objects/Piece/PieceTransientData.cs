using System;
using UnityEngine;

[Serializable]
public class PieceTransientData
{
    [SerializeField]
    private SerializableGuid _id;
    
    public PieceTransientData(Guid id, Vector3 localPosition, PieceColor[] colors, PieceRotation rotation, float creationTime, Vector3 worldPosition)
    {
        _id = id;
        LocalPosition = localPosition;
        Colors = colors;
        Rotation = rotation;
        CreationTime = creationTime;
        WorldPosition = worldPosition;
    }

    public Guid Id => _id;
    
    [field: SerializeField]
    public Vector3 LocalPosition { get; private set; }
    
    [field: SerializeField]
    public Vector3 WorldPosition { get; private set; }
    
    [field: SerializeField]
    public PieceRotation Rotation { get; private set; }
    
    [field: SerializeReference]
    public PieceColor[] Colors { get; private set; }
    
    [field: SerializeField]
    public float CreationTime { get; private set; }
}
