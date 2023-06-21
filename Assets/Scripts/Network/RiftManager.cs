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
    public RiftVariableSyncManager variableSyncManager;

    /// <summary>
    ///     The unit client we communicate via.
    /// </summary>
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    public UnityClient client;

    public static List<ushort> clients = new List<ushort>();
    
    public static Dictionary<RiftView, GameObject> riftGameObjects = new Dictionary<RiftView, GameObject>();

    public List<GameObject> spawnableObjects = new List<GameObject>();

    
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


        client.MessageReceived += Client_MessageReceived;

        InvokeRepeating("ServerTick", 1.0f, 0.01f);

        //DontDestroyOnLoad(gameObject);
    }


    void ServerTick()
    {
        streamSerializer.SendSerializationSync();
        variableSyncManager.SendSyncProperties();
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
                else if(message.Tag == RiftTags.RemoveObject)
                {
                    RiftView view = reader.ReadSerializable<RiftView>();
                    RiftManager.Instance.DestroyNetworkObject(view);
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

        return obj;
    }

    public void DestroyNetworkObject(RiftView view)
    {
        if (riftGameObjects.ContainsKey(view))
        {
            Destroy(riftGameObjects[view]);
            riftGameObjects.Remove(view);
        }
    }
}


