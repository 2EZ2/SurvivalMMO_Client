using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using System;
using DarkRift.Client.Unity;
using System.Reflection;

public class RiftBehaviour : MonoBehaviour
{
    /// <summary>
    ///     The DarkRift client to send data though.
    /// </summary>
    UnityClient client;

    [SerializeField]
    private RiftView riftView = new RiftView();
   
    public RiftView _RiftView { get => riftView; set => riftView = value; }

    public bool IsMine { get => client.ID == _RiftView.Owner; }


    public virtual RiftStream OnStreamSerializeEvent(RiftStream Stream)
    {
        return Stream;
    }

    public virtual void OnStreamDeserializeEvent(RiftStream Stream)
    {

    }

    public virtual void SendStreamSerializeEvent()
    {

    }

    public void ProcessRPC(RPCDataView view)
    {       
        foreach(MethodInfo info in this.GetType().GetMethods())
        {
            if(info.Name == view.MethodName)
            {
                Debug.Log($@"parameter count:{info.GetParameters().Length}, buffer length: {view.parameterValues.ToArray().Length}");
                info.Invoke(this, view.parameterValues.ToArray());
            }
        }
    }

    public void UpdateSyncVars()
    {
        foreach (PropertyInfo info  in this.GetType().GetRuntimeProperties())
        {
            if(info.GetCustomAttribute<RiftSyncVar>() != null)
            {
                info.SetValue(this, null);
            }
        }
    }

    /// <summary>
    ///     Sets up the character with necessary references.
    /// </summary>
    /// <param name="client">The client to send data using.</param>
    /// <param name="blockWorld">The block world reference.</param>
    public RiftBehaviour Setup(UnityClient client, ushort id, ushort owner)
    {
        this.client = client;
        this._RiftView = new RiftView(id, owner);
        return this;
    }
}


