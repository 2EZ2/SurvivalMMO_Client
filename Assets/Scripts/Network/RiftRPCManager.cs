using DarkRift;
using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class RiftRPCManager : MonoBehaviour
{
    public static Dictionary<string, Type[]> RPC_COMMANDS = new Dictionary<string, Type[]>();

    /// <summary>
    ///     The unit client we communicate via.
    /// </summary>
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    public UnityClient client;

    // Start is called before the first frame update
    void Start()
    {
        client.MessageReceived += Client_MessageReceived;

        GetAllRPCs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Client_MessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                if (message.Tag == RiftTags.RPC)
                {
                    Debug.Log("Recieved Server RPC Command");
                    object convertedStream;

                    RiftView rv = reader.ReadSerializable<RiftView>();

                    string method = reader.ReadString();
                    byte[] bytes = reader.ReadBytes();

                    Debug.Log($@"Got bytes size:{bytes.Length}");

                    IFormatter formatter = new BinaryFormatter();

                    using (MemoryStream memoryStream = new MemoryStream(bytes))
                    {
                        RiftStream stream;

                        convertedStream = formatter.Deserialize(memoryStream);
                        Debug.Log($@"Deserialized stream = {convertedStream}");

                        stream = new RiftStream(false, null);
                        stream.SetReadStream(((object[])convertedStream), 0);

                        RPCDataView dataView = new RPCDataView(rv, method, stream);
                        if (RiftManager.riftGameObjects.ContainsKey(rv))
                        {
                            RiftManager.riftGameObjects[rv].GetComponent<RiftPlayerController>().BroadcastMessage("ProcessRPC", dataView);
                        }
                    }
                }
            }
        }
                
    }

    public static void SendPrivateRPC(RiftView sender, RiftView target, string method, params object[] inputs)
    {
        //Serialize
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(sender.ID.ToByteArray());
            writer.Write(sender.Owner);
            writer.Write(method);
            object[] streamCache = inputs;
            writer.WriteAs(typeof(object), streamCache);
            writer.Write(true); //self exclusion
            writer.Write(sender.ID.ToByteArray());
            writer.Write(sender.Owner);

            Debug.Log($@"Running RPC {method}");
            //Send
            using (Message message = Message.Create(RiftTags.PrivateRPC, writer))
                RiftManager.Instance.client.SendMessage(message, SendMode.Reliable);
        }
    }


    public static void SendRPC(RiftView view, RPCTarget target, string method, params object[] inputs)
    {
        //Serialize
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(view.ID.ToByteArray());
            writer.Write(view.Owner);
            writer.Write(method);
            object[] streamCache = inputs;
            writer.WriteAs(typeof(object), streamCache);

            if (target == RPCTarget.Everyone)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
            }

            Debug.Log($@"Running RPC {method}");
            //Send
            using (Message message = Message.Create(RiftTags.RPC, writer))
                RiftManager.Instance.client.SendMessage(message, SendMode.Reliable);
        }
    }

    void GetAllRPCs()
    {
        RPC_COMMANDS.Clear();
        var types = Assembly.GetAssembly(typeof(RiftBehaviour)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RiftBehaviour)));
        foreach (var field in types)
        {
            foreach (MethodInfo info in field.GetMethods())
            {
                if (info.GetCustomAttribute<RiftRPC>() != null)
                {
                    Debug.Log($@"Found RPC: {info.Name} in {field.Name}");

                    if (!RPC_COMMANDS.ContainsKey(info.Name))
                    {
                        List<Type> ptypes = new List<Type>();

                        foreach (var item in info.GetParameters())
                        {
                            Debug.Log($@"RPC:{info.Name} has parameter {item.ParameterType}");
                            ptypes.Add(item.ParameterType);
                        }

                        RPC_COMMANDS.Add(info.Name, ptypes.ToArray());
                    }
                }
            }
        }
    }

}
