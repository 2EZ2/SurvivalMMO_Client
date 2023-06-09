using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using System;
using DarkRift.Client.Unity;
using System.Reflection;

[RequireComponent(typeof(RiftIdentity))]
public class RiftBehaviour : MonoBehaviour
{
    public RiftIdentity Identity { get; set; }

    public bool IsMine { get => Identity?.IsMine ?? false; }

    private MethodInfo[] methodsCache { get; set; }

    public void ProcessRPC(RPCDataView view)
    {       
        if(methodsCache == null)
        {
            methodsCache = this.GetType().GetMethods();
        }

        foreach(MethodInfo info in methodsCache)
        {
            if(info.Name == view.MethodName)
            {
                info.Invoke(this, view.Inputs);
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

   

}


