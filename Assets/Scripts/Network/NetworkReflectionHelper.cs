using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class NetworkReflectionHelper : MonoBehaviour
{
    public Dictionary<string, Type[]> GetAllRPCs()
    {
        Dictionary<string, Type[]> RPC_COMMANDS = new Dictionary<string, Type[]>();
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

        return RPC_COMMANDS;
    }


    public Dictionary<string, List<Type>> GetAllSyncVars()
    {
        Dictionary<string, List<Type>> Behaviour_SyncVars = new Dictionary<string, List<Type>>();

        var types = Assembly.GetAssembly(typeof(IRiftSerializable)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RiftBehaviour)));

        foreach (var field in types)
        {
            foreach (PropertyInfo info in field.GetProperties())
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
                        if (!Behaviour_SyncVars[field.Name].Contains(info.PropertyType))
                        {
                            Behaviour_SyncVars[field.Name].Add(info.PropertyType);
                        }
                    }        
                }              
            }            
        }

        return Behaviour_SyncVars;
    }



}
