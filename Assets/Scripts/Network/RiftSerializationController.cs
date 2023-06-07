using DarkRift;
using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftSerializationController : MonoBehaviour
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
                                RiftManager.riftGameObjects[_message.View].GetComponent<RiftPlayerController>().OnStreamDeserializeEvent(stream);
                            }
                        }
                    }
                }
            } 
        }
                
    }

    public static void SendMessages(RiftView view, List<RiftMessage> messages)
    {
        //Serialize
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            foreach (RiftMessage _message in messages)
            {
                writer.Write<RiftMessage>(_message);
            }

            //Send
            using (Message message = Message.Create(RiftTags.SendStream, writer))
                RiftManager.Instance.client.SendMessage(message, SendMode.Unreliable);
        }
    }

    public void SendSerializationSync()
    {
        List<RiftMessage> messages = new List<RiftMessage>();

        RiftBehaviour[] behaviours = GameObject.FindObjectsByType<RiftBehaviour>(FindObjectsSortMode.InstanceID);
        var type = typeof(IRiftSerializable);

        //Seralize event
        foreach (var rb in behaviours)
        {
            if (type.IsAssignableFrom(rb.GetType()))
            {
                if (rb._RiftView.Owner == client.ID)
                {
                    IRiftSerializable riftSerializable = (IRiftSerializable)rb;

                    if (riftSerializable != null)
                    {
                        RiftMessage newMessage = new RiftMessage(rb._RiftView);

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
