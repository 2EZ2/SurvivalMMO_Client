using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RiftIdentity : MonoBehaviour
{
    /// <summary>
    ///     The DarkRift client to send data though.
    /// </summary>
    public UnityClient client;

    [SerializeField]
    public RiftView View { get => view; set => view = value; }

    public Guid ID { get => View.ID; }

    [SerializeField]
    public ushort Owner { get => View.Owner; }

    public bool IsMine { get => client.ID == View.Owner; }
    public List<IRiftSerializable> RiftSerializables { get => riftSerializables; set => riftSerializables = GetSerializableClasses(); }

    [SerializeField]
    private List<IRiftSerializable> riftSerializables = new List<IRiftSerializable>();

    [SerializeField]
    private RiftView view = new RiftView();

    /// <summary>
    ///     Sets up the character with necessary references.
    /// </summary>
    /// <param name="client">The client to send data using.</param>
    /// <param name="blockWorld">The block world reference.</param>
    public RiftIdentity Setup(UnityClient client, RiftView view)
    {
        this.client = client;
        this.View = view;
        return this;
    }

    public List<IRiftSerializable> GetSerializableClasses()
    {
        if(riftSerializables.Count > 0)
        {
            return riftSerializables;
        }

        List<IRiftSerializable> classes = new List<IRiftSerializable>();

        RiftBehaviour[] behaviours = gameObject.GetComponents<RiftBehaviour>();
        var type = typeof(IRiftSerializable);

        //Seralize event
        foreach (var rb in behaviours)
        {
            if (type.IsAssignableFrom(rb.GetType()))
            {
                if (rb.Identity.Owner == client.ID)
                {
                    IRiftSerializable riftSerializable = (IRiftSerializable)rb;

                    if (riftSerializable != null)
                    {
                        classes.Add(riftSerializable);
                    }
                }
            }
        }

        riftSerializables = classes;

        return classes;
    }
}
