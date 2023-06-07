using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Property |
                       System.AttributeTargets.Struct,
                       AllowMultiple = true)]// Multiuse attribute.
public class RiftSyncVar : Attribute
{
    public object TargetObject = null;
}

[System.AttributeUsage(System.AttributeTargets.Method,
                       AllowMultiple = true)]// Multiuse attribute.
public class RiftRPC : Attribute
{
    
}
