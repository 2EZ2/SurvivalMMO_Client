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
    /// <summary>
    ///     The unit client we communicate via.
    /// </summary>
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    public UnityClient client;

    public RiftView localPlayer = new RiftView(0, 0);

    Dictionary<ushort, RiftView> players = new Dictionary<ushort, RiftView>();
    Dictionary<ushort, RiftView> riftObjects = new Dictionary<ushort, RiftView>();
    
    Dictionary<RiftView, GameObject> riftGameObjects = new Dictionary<RiftView, GameObject>();
    /// <summary>
    ///     The player object to spawn for our player.
    /// </summary>
    [SerializeField]
    [Tooltip("The player object to spawn.")]
    GameObject playerPrefab;

    /// <summary>
    ///     The player object to spawn for others' players.
    /// </summary>
    [SerializeField]
    [Tooltip("The network player object to spawn.")]
    GameObject networkPlayerPrefab;

    public static Dictionary<string, Type[]> RPC_COMMANDS = new Dictionary<string, Type[]>();

    public static Dictionary<RiftView, Type> View_Behaviour = new Dictionary<RiftView, Type>();

    public static Dictionary<string, List<Type>> Behaviour_SyncVars = new Dictionary<string, List<Type>>();



    public static void SendSync(RiftView view, RiftStream stream)
    {
        //Serialize
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(view.ID);
            writer.Write(view.Owner);
            object[] streamCache = stream.GetWriteStream().ToArray();
            writer.WriteAs(typeof(object), streamCache);
            
            //Send
            using (Message message = Message.Create(RiftTags.SendStream, writer))
               RiftManager.Instance.client.SendMessage(message, SendMode.Unreliable);
        }
    }

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

        localPlayer.ID = client.ID;
        localPlayer.Owner = client.ID;
        Debug.Log(client.ID);
        
        
        var types = Assembly.GetAssembly(typeof(RiftBehaviour)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RiftBehaviour)));
        foreach (var field in types)
        {
            foreach (PropertyInfo info in typeof(RiftBehaviour).GetProperties())
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
        
        client.MessageReceived += Client_MessageReceived;

        //DontDestroyOnLoad(gameObject);
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
                Debug.Log($@"Recieved server message tag:{message.Tag}");
                if (message.Tag == RiftTags.PlayerConnected)
                {
                    ushort id = reader.ReadUInt16();
                    ushort owner = reader.ReadUInt16();
                    RiftView view = new RiftView(id, owner);

                    GameObject obj = Instantiate(playerPrefab);
                    RiftBehaviour behaviour = obj.GetComponent<RiftPlayerController>().Setup(client, id, owner);

                    if (!riftGameObjects.ContainsKey(view))
                    {
                        riftGameObjects.Add(view, obj);
                    }

                    if (!players.ContainsKey(id))
                    {
                        players.Add(id, view);
                    }

                    if (!View_Behaviour.ContainsKey(view))
                    {
                        View_Behaviour.Add(view, behaviour.GetType());
                    }
                                       
                }
                else if (message.Tag == RiftTags.PlayerDisconnected)
                {
                    ushort id = reader.ReadUInt16();
                    ushort owner = reader.ReadUInt16();

                    if (players.ContainsKey(id))
                    {
                        players.Remove(id);
                    }
                }
                else if (message.Tag == RiftTags.SpawnPlayer)
                {
                    ushort id = reader.ReadUInt16();
                    ushort owner = reader.ReadUInt16();
                    RiftView view = new RiftView(id, owner);

                    GameObject obj = Instantiate(playerPrefab);
                    RiftBehaviour behaviour = obj.GetComponent<RiftPlayerController>().Setup(client, id, owner);

                    if (!riftGameObjects.ContainsKey(view))
                    {
                        riftGameObjects.Add(view, obj);
                    }

                    if (!players.ContainsKey(id))
                    {
                        players.Add(id, view);
                    }

                    if (!View_Behaviour.ContainsKey(view))
                    {
                        View_Behaviour.Add(view, behaviour.GetType());
                    }

                }
                else if (message.Tag == RiftTags.ReceivingStream)
                {
                    Debug.Log("Recieved Server Message to stream");
                    object convertedStream;

                    ushort id = reader.ReadUInt16();
                    ushort owner = reader.ReadUInt16();
                    byte[] bytes = reader.ReadBytes();

                    Debug.Log($@"Got bytes size:{bytes.Length}");

                    IFormatter formatter = new BinaryFormatter();
                    RiftView rv = new RiftView(id, owner);

                    using (MemoryStream memoryStream = new MemoryStream(bytes))
                    {
                        RiftStream stream;

                        convertedStream = formatter.Deserialize(memoryStream);
                        Debug.Log($@"Deserialized stream = {convertedStream}");
                        if (id != client.ID)
                        {
                            stream = new RiftStream(false, null);
                            stream.SetReadStream(((object[])convertedStream),0);

                            if (riftGameObjects.ContainsKey(rv))
                            {
                                riftGameObjects[rv].GetComponent<RiftPlayerController>().OnStreamDeserializeEvent(stream);
                            }                            
                        }                        
                    }                                     
                }
            }
        }
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
            ushort id = reader.ReadUInt16();
            ushort owner = reader.ReadUInt16();
            return new RiftView(id, owner);
        }

        return null;
    }


}
