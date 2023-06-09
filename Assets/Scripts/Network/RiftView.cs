using DarkRift;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
///     Holds serializable data about a player.
/// </summary>
[System.Serializable]
public class RiftView : IDarkRiftSerializable, ISerializable
{
    public Guid ID { get; set; }
    public ushort Owner { get; set; }

    public RiftView()
    {
        ID = Guid.NewGuid();
        Owner = 100;
    }
    public RiftView(Guid id, ushort owner)
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
        return (int)(ID.GetHashCode() + this.Owner.GetHashCode());
    }

    public static void SendPrivateRPC(RiftView sender, RiftView target, string type, string method, object[] inputs)
    {
        //Serialize
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            RPCDataView dataView = new RPCDataView(sender, target, type, method, false, inputs);

            writer.Write<RPCDataView>(dataView);

            Debug.Log($@"Running RPC {method}");
            //Send
            using (Message message = Message.Create(RiftTags.PrivateRPC, writer))
                RiftManager.Instance.client.SendMessage(message, SendMode.Reliable);
        }
    }

    public static void SendRPC(RiftView sender, RPCTarget target, string type, string method, object[] inputs)
    {
        //Serialize
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            RPCDataView dataView;

            if (target == RPCTarget.Everyone)
            {
                dataView = new RPCDataView(sender, sender, type, method, true, inputs);
            }
            else
            {
                dataView = new RPCDataView(sender, sender, type, method, false, inputs);
            }

            writer.Write<RPCDataView>(dataView);

            Debug.Log($@"Running RPC {method}");
            //Send
            using (Message message = Message.Create(RiftTags.RPC, writer))
                RiftManager.Instance.client.SendMessage(message, SendMode.Reliable);
        }
    }

    public void RPC(string componentName, string methodName, RPCTarget target, params object[] inputs)
    {
        SendRPC(this, target, componentName, methodName, inputs);
    }

    public void RPC(string componentName, string methodName, RiftView targetView, params object[] inputs)
    {
        SendPrivateRPC(this, targetView, componentName, methodName, inputs);
    }

    public void Deserialize(DeserializeEvent e)
    {
        byte[] id = e.Reader.ReadBytes();        
        ID = new Guid(id);
        Owner = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ID.ToByteArray());
        e.Writer.Write(Owner);
    }

    public RiftView(SerializationInfo info, StreamingContext ctxt)
    {
        ID = (Guid)info.GetValue("ID", typeof(Guid));
        Owner = (ushort)info.GetValue("OWN", typeof(ushort));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("ID", ID);
        info.AddValue("OWN", Owner);
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
