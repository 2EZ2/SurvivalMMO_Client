using DarkRift;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class RiftMessage : IDarkRiftSerializable, ISerializable
{
    public RiftView View { get; set; }

    public byte[] data { get; set; }

    public object[] message { get; set; }

    public RiftMessage()
    {
        View = new RiftView(new Guid(), 100);
    }

    public RiftMessage(RiftView view)
    {
        View = view;
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(View.ID.ToByteArray());
        e.Writer.Write(View.Owner);

        IFormatter formatter = new BinaryFormatter();

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, message);
            data = stream.ToArray();

            if (data != null)
            {
                e.Writer.Write(data);
            }
            else
            {
                e.Writer.Write(new byte[] { 0,0,0,0});
            }
        }
    }

    public void Deserialize(DeserializeEvent e)
    {
        object convertedStream;

        View = e.Reader.ReadSerializable<RiftView>();

        data = e.Reader.ReadBytes();

        IFormatter formatter = new BinaryFormatter();

        using (MemoryStream memoryStream = new MemoryStream(data))
        {
            convertedStream = formatter.Deserialize(memoryStream);
            message = (object[])convertedStream;
        }        
    }

    public RiftMessage(SerializationInfo info, StreamingContext ctxt)
    {
        View.ID = (Guid)info.GetValue("ID", typeof(Guid));
        View.Owner = (ushort)info.GetValue("OWN", typeof(ushort));
        message = (object[])info.GetValue("DATA", typeof(object[]));   
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("ID", View.ID);
        info.AddValue("OWN", View.Owner);
        info.AddValue("DATA", message);
    }
}
