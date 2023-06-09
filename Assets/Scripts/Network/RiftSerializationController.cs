using DarkRift;
using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class RiftSerializationController : MonoBehaviour
{
    /// <summary>
    ///     The unit client we communicate via.
    /// </summary>
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    public UnityClient client;


    private TypeInfo[] cachedRiftSerializableTypes;

    // Start is called before the first frame update
    void Start()
    {
        if (client == null)
        {
            Debug.LogError("No client assigned to BlockPlayerSpawner component!");
            return;
        }

        client.MessageReceived += Client_MessageReceived;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Client_MessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        using(Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                if (message.Tag == RiftTags.ReceivingStream)
                {
                    RiftMessage[] messages = reader.ReadSerializables<RiftMessage>();

                    foreach (RiftMessage _message in messages)
                    {
                        if (_message.View.Owner != client.ID)
                        {
                            RiftStream stream = new RiftStream(false, null);
                            stream.SetReadStream(_message.message, 0);

                            if (RiftManager.riftGameObjects.ContainsKey(_message.View))
                            {
                                var tempComp = RiftManager.riftGameObjects[_message.View].GetComponent(_message.SystemType);

                                if (tempComp != null)
                                {
                                    ((IRiftSerializable)tempComp)?.OnStreamDeserializeEvent(stream);                                    
                                }
                            }
                        }
                    }
                }
            } 
        }
                
    }

    public void SendSerializationSync()
    {
        if (cachedRiftSerializableTypes == null)
        {
            cachedRiftSerializableTypes = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(x => typeof(IRiftSerializable).IsAssignableFrom(x) && !x.IsInterface)
                .ToArray<TypeInfo>();
        }

        List<RiftMessage> messages = new List<RiftMessage>();

        foreach(var rObject in RiftManager.riftGameObjects)
        {
            if (rObject.Key.Owner == client.ID)
            {
                //Seralize event
                foreach (var dt in cachedRiftSerializableTypes)
                {
                    var serializableType = rObject.Value.GetComponent(dt.AsType());
                    if (serializableType != null)
                    {
                        IRiftSerializable riftSerializable = (IRiftSerializable)serializableType;

                        if (riftSerializable != null)
                        {
                            RiftMessage newMessage = new RiftMessage(rObject.Key, dt.AsType().Name);

                            RiftStream stream = new RiftStream(true, null);
                            riftSerializable.OnStreamSerializeEvent(stream);

                            newMessage.message = stream.GetWriteStream().ToArray();

                            if (newMessage.message.Length > 0)
                            {
                                messages.Add(newMessage);
                            }
                        }
                    }
                }                
            }
        }

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((ushort)messages.Count);

            for (int i = 0; i < messages.Count; i++)
            {
                writer.Write(messages[i]);
            }

            using (Message message = Message.Create(RiftTags.SendStream, writer))
                RiftManager.Instance.client.SendMessage(message, SendMode.Unreliable);
        }
    }

}
