using System;
using UnityEngine;

[Serializable]
public struct SerializableGuid
{
    [SerializeField]
    private byte[] _value;

    private SerializableGuid(byte[] value) => _value = value;

    public static implicit operator Guid(SerializableGuid guid) => new(guid._value);
    public static implicit operator SerializableGuid(Guid guid) => new(guid.ToByteArray());
}
