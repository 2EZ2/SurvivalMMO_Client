using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

public class RiftManager : MonoBehaviour
{
    public static RiftManager Instance { get; private set; }

    public RiftSerializationController streamSerializer;

    /// <summary>
    ///     The unit client we communicate via.
    /// </summary>
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    public UnityClient client;

    public static List<ushort> clients = new List<ushort>();
    
    public static Dictionary<RiftView, GameObject> riftGameObjects = new Dictionary<RiftView, GameObject>();
    
    public static Dictionary<RiftView, Type> View_Behaviour = new Dictionary<RiftView, Type>();

    public List<GameObject> spawnableObjects = new List<GameObject>();

    public static Dictionary<string, List<Type>> Behaviour_SyncVars = new Dictionary<string, List<Type>>();
    
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        if (client == null)
        {
            Debug.LogError("No client assigned to BlockPlayerSpawner component!");
            return;
        }

        //localPlayer.ID = client.ID;
        //localPlayer.Owner = client.ID;
        Debug.Log(client.ID);

        GetAllSyncVars();

        client.MessageReceived += Client_MessageReceived;

        InvokeRepeating("ServerTick", 1.0f, 0.01f);

        //DontDestroyOnLoad(gameObject);
    }


    void GetAllSyncVars()
    {
        var types = Assembly.GetAssembly(typeof(RiftBehaviour)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RiftBehaviour)));
        foreach (var field in types)
        {
            foreach (PropertyInfo info in field.GetProperties())
            {
                if (info.GetCustomAttribute<RiftSyncVar>() != null)
                {
                    Debug.Log($@"sync var {info.PropertyType} {info.Name} in {field.Name}");

                    if (!Behaviour_SyncVars.ContainsKey(field.Name))
                    {
                        List<Type> type = new List<Type>();
                        type.Add(info.PropertyType);
                        Behaviour_SyncVars.Add(field.Name, type);
                    }
                    else
                    {
                        if (Behaviour_SyncVars.ContainsKey(field.Name))
                        {
                            if (!Behaviour_SyncVars[field.Name].Contains(info.PropertyType))
                            {
                                Behaviour_SyncVars[field.Name].Add(info.PropertyType);
                            }
                        }
                    }
                }
            }
        }
        }

    void ServerTick()
    {
        streamSerializer.SendSerializationSync();
    }

    public void SendSyncVars()
    {
        RiftBehaviour[] behaviours = GameObject.FindObjectsByType<RiftBehaviour>(FindObjectsSortMode.InstanceID);
        //Get All class of type Rift Behaviour
        var types = Assembly.GetAssembly(typeof(RiftBehaviour)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RiftBehaviour)));

        //Send sync var loop
        foreach (var field in types)
        {
            foreach (PropertyInfo info in field.GetProperties())
            {
                if (info.GetCustomAttribute<RiftSyncVar>() != null)
                {
                    Debug.Log($@"sync var {info.PropertyType} {info.Name} in {field.Name}");

                    foreach (var rb in behaviours)
                    {
                        if (field.IsAssignableFrom(rb.GetType()))
                        {
                            if(rb.Identity.Owner == client.ID)
                            {

                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Called when a message is received from the server.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Client_MessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                //Read message
                if (message.Tag == RiftTags.PlayerConnected)
                {
                    ushort id = reader.ReadUInt16();
                    if (clients.Contains(id))
                    {
                        clients.Add(id);
                    }
                }
                else if (message.Tag == RiftTags.PlayerDisconnected)
                {
                    ushort id = reader.ReadUInt16();

                    if (clients.Contains(id))
                    {
                        clients.Remove(id);
                    }
                }
                else if (message.Tag == RiftTags.InsantiateObject)
                {
                    RiftView view = reader.ReadSerializable<RiftView>();
                    ushort index = reader.ReadUInt16();

                    RiftManager.Instance.NetworkSpawn(index, transform.position, view);
                }
            }
        }
    }

    public GameObject NetworkSpawn(ushort prefabIndex, Vector3 position, RiftView view)
    {
        GameObject obj = Instantiate(spawnableObjects[prefabIndex]);
        RiftIdentity behaviour = obj.GetComponent<RiftIdentity>().Setup(client, view);

        if (!riftGameObjects.ContainsKey(view))
        {
            riftGameObjects.Add(view, obj);
        }

        if (!View_Behaviour.ContainsKey(view))
        {
            View_Behaviour.Add(view, behaviour.GetType());
        }

        return obj;
    }
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
