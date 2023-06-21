using DarkRift;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class StreamExtentions 
{
 
}

public static class DarkRiftReaderExtension
{
    public static void WriteAs(this DarkRiftWriter writer, Type type, object input)
    {
        if (type == typeof(float))
        {
            writer.Write((float)input);
        }
        if (type == typeof(float[]))
        {
            writer.Write((float[])input);
        }
        else if (type == typeof(double))
        {
            writer.Write((double)input);
        }
        else if (type == typeof(double[]))
        {
            writer.Write((double[])input);
        }
        else if (type == typeof(bool))
        {
            writer.Write((bool)input);
        }
        else if (type == typeof(bool[]))
        {
            writer.Write((bool[])input);
        }
        else if (type == typeof(byte))
        {
            writer.Write((byte)input);
        }
        else if (type == typeof(byte[]))
        {
            writer.Write((byte[])input);
        }
        else if (type == typeof(char))
        {
            writer.Write((char)input);
        }
        else if (type == typeof(char[]))
        {
            writer.Write((char[])input);
        }
        else if (type == typeof(string))
        {
            writer.Write((string)input);
        }
        else if (type == typeof(string[]))
        {
            writer.Write((string[])input);
        }
        else if (type == typeof(Int16))
        {
            writer.Write((Int16)input);
        }
        else if (type == typeof(Int16[]))
        {
            writer.Write((Int16[])input);
        }
        else if (type == typeof(int))
        {
            writer.Write((Int64)input);
        }
        else if (type == typeof(int[]))
        {
            writer.Write((Int64[])input);
        }
        else if (type == typeof(UInt16))
        {
            writer.Write((UInt16)input);
        }
        else if (type == typeof(UInt16[]))
        {
            writer.Write((UInt16[])input);
        }
        else if (type == typeof(ushort))
        {
            writer.Write((ushort)input);
        }
        else if (type == typeof(Vector3))
        {
            Vector3 vector = (Vector3)input;
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }
        else if (type == typeof(object))
        {
            byte[] bytes;
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, input);
                bytes = stream.ToArray();

                if (bytes != null)
                {
                    writer.Write(bytes);
                }
            }
        }
    }


    public static object ReadAs(this DarkRiftReader reader, Type type)
    {
        if (type == typeof(float))
        {
            return reader.ReadSingle();
        }
        if (type == typeof(float[]))
        {
            return reader.ReadSingles();
        }
        else if (type == typeof(double))
        {
            return reader.ReadDouble();
        }
        else if (type == typeof(double[]))
        {
            return reader.ReadDoubles();
        }
        else if (type == typeof(bool))
        {
            return reader.ReadBoolean();
        }
        else if (type == typeof(bool[]))
        {
            return reader.ReadBooleans();
        }
        else if (type == typeof(byte))
        {
            return reader.ReadByte();
        }
        else if (type == typeof(byte[]))
        {
            return reader.ReadBytes();
        }
        else if (type == typeof(char))
        {
            return reader.ReadChar();
        }
        else if (type == typeof(char[]))
        {
            return reader.ReadChars();
        }
        else if (type == typeof(string))
        {
            return reader.ReadString();
        }
        else if (type == typeof(string[]))
        {
            return reader.ReadStrings();
        }
        else if (type == typeof(Int16))
        {
            return reader.ReadInt16();
        }
        else if (type == typeof(Int16[]))
        {
            return reader.ReadInt16();
        }
        else if (type == typeof(int))
        {
            return reader.ReadInt64();
        }
        else if (type == typeof(int[]))
        {
            return reader.ReadInt64s();
        }
        else if (type == typeof(UInt16))
        {
            return reader.ReadUInt16();
        }
        else if (type == typeof(UInt16[]))
        {
            return reader.ReadUInt16s();
        }
        else if (type == typeof(ushort))
        {
            return reader.ReadUInt16();
        }
        else if (type == typeof(Vector3))
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }
        else if (type == typeof(object))
        {
            byte[] bytes = reader.ReadBytes();
            IFormatter formatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                return formatter.Deserialize(memoryStream);
            }
        }
        else if (type == typeof(RiftView))
        {
            byte[] id = reader.ReadBytes();
            ushort owner = reader.ReadUInt16();
            return new RiftView(new Guid(id), owner);
        }

        return null;
    }

}
