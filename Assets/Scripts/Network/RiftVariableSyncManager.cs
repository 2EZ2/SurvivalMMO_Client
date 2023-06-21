using DarkRift;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


public class RiftVariableSyncManager : MonoBehaviour
{
    /// <summary>
    ///     The unit client we communicate via.
    /// </summary>
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    public UnityClient client;


    List<TypeInfo> cachedRiftSyncableTypes = new List<TypeInfo>();
    Dictionary<string, Dictionary<string, string>> syncVars = new Dictionary<string, Dictionary<string, string>>();


    // Start is called before the first frame update
    void Start()
    {
        GetCachedSyncableClasses();
        client.MessageReceived += Client_MessageReceived;
    }

    // Update is called once per frame


    public void GetCachedSyncableClasses()
    {

        if (cachedRiftSyncableTypes == null)
        {
            cachedRiftSyncableTypes = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(x => typeof(RiftBehaviour).IsAssignableFrom(x) && !x.IsAbstract)
                .ToList<TypeInfo>();
        }

        foreach(var type in cachedRiftSyncableTypes)
        {
            foreach (var item in type.GetProperties())
            {
                var tmp = item.GetCustomAttribute<SyncVar>();

                if (tmp != null)
                {
                    if(!syncVars.ContainsKey(type.Name))
                    {
                        syncVars.Add(type.Name, new Dictionary<string, string>());
                    }
                    
                    if (tmp.PropertyToSyncTo != "")
                    {
                        syncVars[type.Name].Add(item.Name, tmp.PropertyToSyncTo);
                    }
                    else
                    {
                        syncVars[type.Name].Add(item.Name, item.Name);
                    }
                }
            }
        }
        
    }

    public void SendSyncProperties()
    {
        List<VarSyncDataView> messages = new List<VarSyncDataView>();

        foreach (var rObject in RiftManager.riftGameObjects)
        {
            if (rObject.Key.Owner == client.ID)
            {
                foreach (var item in cachedRiftSyncableTypes)
                {
                    Component comp = rObject.Value.GetComponent(item.Name);

                    if (comp != null)
                    {
                        foreach (var _var in syncVars[item.Name])
                        {
                            PropertyInfo syncVar = item.GetProperty(_var.Key);
                            //PropertyInfo linkVar = item.GetProperty(_var.Value);

                            if (syncVar != null)
                            {
                                VarSyncDataView dataView = new VarSyncDataView(rObject.Key, item.Name, syncVar.Name, syncVar.GetValue(comp));
                                messages.Add( dataView );   

                            }
                        }
                    }
                }
            }
        }

        if(messages.Count > 0)
        {
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

    void Client_MessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                if (message.Tag == RiftTags.ReceivingStream)
                {
                    VarSyncDataView[] messages = reader.ReadSerializables<VarSyncDataView>();

                    foreach (VarSyncDataView _message in messages)
                    {
                        if (_message.SenderView.Owner != client.ID)
                        {                         
                            if (RiftManager.riftGameObjects.ContainsKey(_message.SenderView))
                            {
                                var tempComp = RiftManager.riftGameObjects[_message.SenderView].GetComponent(_message.SystemType);

                                if (tempComp != null)
                                {
                                    PropertyInfo syncVar = tempComp.GetType().GetProperty(syncVars[_message.SystemType][_message.PropertyName]);

                                    syncVar?.SetValue(tempComp, _message.Value);
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public void UpdateSyncProperty(object target, string propertyToUpdate, object value)
    {

    }
}

[System.Serializable]
public class VarSyncDataView : IDarkRiftSerializable
{
    public RiftView SenderView { get; set; }

    public string SystemType { get; set; }

    public string PropertyName { get; set; }

    public object Value { get; set; }

    public VarSyncDataView()
    {
        this.SenderView = new RiftView();
        this.SystemType = "";
        this.PropertyName = "";
        this.Value = new object();
    }

    public VarSyncDataView(RiftView sender, string componentName, string propertyName, object input)
    {
        this.SenderView = sender;
        this.SystemType = componentName;
        this.PropertyName = propertyName;
        this.Value = input;
    }

    public void Deserialize(DeserializeEvent e)
    {
        SenderView = e.Reader.ReadSerializable<RiftView>();
        SystemType = e.Reader.ReadString();
        PropertyName = e.Reader.ReadString();

        byte[] bytes = e.Reader.ReadBytes();

        IFormatter formatter = new BinaryFormatter();

        using (MemoryStream memoryStream = new MemoryStream(bytes))
        {
            object convertedStream = formatter.Deserialize(memoryStream);

            this.Value = ((object)convertedStream);
        }
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write<RiftView>(this.SenderView);
        e.Writer.Write(this.SystemType);
        e.Writer.Write(this.PropertyName);
        e.Writer.WriteAs(typeof(object), Value);
    }
}

public class SyncVar : Attribute
{
    public string PropertyToSyncTo { get; set; }
}
