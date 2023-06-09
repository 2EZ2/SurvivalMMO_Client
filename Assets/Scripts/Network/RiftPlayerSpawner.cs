using DarkRift;
using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftPlayerSpawner : MonoBehaviour
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
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                if (message.Tag == RiftTags.PlayerConnected)
                {
                    ushort clientID = reader.ReadUInt16();
                    if(clientID == client.ID)
                    {
                        SendSpawnRequest(); //Send Spawn request when I join
                    }                                      
                }
                
            }
        }
    }


    public static void SendSpawnRequest()
    {
        //Serialize
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write((ushort)0);

            //Send
            using (Message message = Message.Create(RiftTags.InsantiateObject, writer))
                RiftManager.Instance.client.SendMessage(message, SendMode.Unreliable);
        }
    }
}
