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
                    RPCDataView dataView = reader.ReadSerializable<RPCDataView>();

                    Debug.Log("Recieved Server RPC Command");

                    if (RiftManager.riftGameObjects.ContainsKey(dataView.TargetRiftView))
                    {
                        var tempComponent = RiftManager.riftGameObjects[dataView.TargetRiftView].GetComponent(dataView.SystemType);

                        if(tempComponent != null)
                        {
                            tempComponent.BroadcastMessage("ProcessRPC", dataView);
                        }   
                            
                    }
                }
            }
        }
                
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
}

public enum RPCTarget { Everyone, EveryoneElse }

[System.Serializable]
public class RPCDataView : IDarkRiftSerializable
{
    public RiftView SenderView { get; set; }
    public RiftView TargetRiftView { get; set; }

    public string SystemType { get; set; }

    public string MethodName { get; set; }

    public bool RepeatToClient { get; set; }

    public object[] Inputs { get; set; }

    public RPCDataView()
    {
        this.SenderView = new RiftView();
        this.TargetRiftView = new RiftView();
        this.SystemType = "";
        this.MethodName = "";
        this.RepeatToClient = true;
        this.Inputs = new object[1];
    }

    public RPCDataView(RiftView sender, RiftView targetRiftView, string componentName, string methodName, bool repeatToClient, object[] inputs)
    {
        this.SenderView = sender;
        this.TargetRiftView = targetRiftView;
        this.SystemType = componentName;
        this.MethodName = methodName;
        this.RepeatToClient = repeatToClient;
        this.Inputs = inputs;
    }

    public void Deserialize(DeserializeEvent e)
    {
        SenderView = e.Reader.ReadSerializable<RiftView>();
        TargetRiftView = e.Reader.ReadSerializable<RiftView>();
        SystemType = e.Reader.ReadString();
        MethodName = e.Reader.ReadString();
        RepeatToClient = e.Reader.ReadBoolean();

        byte[] bytes = e.Reader.ReadBytes();

        IFormatter formatter = new BinaryFormatter();

        using (MemoryStream memoryStream = new MemoryStream(bytes))
        {
            object convertedStream = formatter.Deserialize(memoryStream);

            this.Inputs = ((object[])convertedStream);
        }
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write<RiftView>(this.SenderView);
        e.Writer.Write<RiftView>(this.TargetRiftView);
        e.Writer.Write(this.SystemType);
        e.Writer.Write(this.MethodName);
        e.Writer.Write(this.RepeatToClient);
        e.Writer.WriteAs(typeof(object), Inputs);
    }
}
