using DarkRift;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
///     Holds serializable data about a player.
/// </summary>
[System.Serializable]
public class RiftView
{
    public ushort ID { get; set; }
    public ushort Owner { get; set; }

    public RiftView()
    {
        ID = 99;
        Owner = 100;
    }
    public RiftView(ushort id, ushort owner)
    {
        ID = id;
        Owner = owner;
    }

    /// <summary>
    ///     Compares an object for equality with this.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>Whether the object is equal to this block.</returns>
    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is RiftView))
            return false;

        RiftView b = (RiftView)obj;

        return this.ID == b.ID && this.Owner == b.Owner;
    }
    /// <summary>
    ///     Basic hashcode generator based on the object id
    /// </summary>
    /// <returns>hash code.</returns>
    public override int GetHashCode()
    {
        return (int)(this.ID * 23 + this.Owner * 29);
    }

    public void RPC(string methodName, RPCTarget target, params object[] inputs)
    {
        RiftManager.SendRPC(this, target, methodName, inputs);
    }

    public void RPC(string methodName, RiftView targetView, params object[] inputs)
    {
        RiftManager.SendRPC(this, RPCTarget.EveryoneElse, methodName, inputs);
    }
}

public enum RPCTarget { Everyone, EveryoneElse}

[System.Serializable]
public class RPCDataView
{
    public RiftView targetRiftView { get; set; }
    public string MethodName { get; set; }

    public RiftStream parameterValues { get; set; }

    public RPCDataView(RiftView targetRiftView, string methodName, RiftStream parameterValues)
    {
        this.targetRiftView = targetRiftView;
        MethodName = methodName;
        this.parameterValues = parameterValues;
    }
}

[System.Serializable]
public class vec3 : ISerializable
{
    public float x;
    public float y;
    public float z;

    public static implicit operator Vector3(vec3 v) => new Vector3(v.x,v.y,v.z);
    public static explicit operator vec3(Vector3 v) => new vec3(v);

    public vec3()
    {
        x = 0f;
        y = 0f;
        z = 0f;
    }

    public vec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public vec3(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }

    public vec3(SerializationInfo info, StreamingContext ctxt)
    {
        x = (float)info.GetValue("x", typeof(float));
        y = (float)info.GetValue("y", typeof(float));
        z = (float)info.GetValue("z", typeof(float));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("x", x);
        info.AddValue("y", y);
        info.AddValue("z", z);
    }
}
