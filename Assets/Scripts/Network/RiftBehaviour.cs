using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using System;
using DarkRift.Client.Unity;

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

    public virtual void OnStreamSerializeEvent(RiftStream Stream)
    {

    }
    public virtual void OnStreamDeserializeEvent(RiftStream Stream)
    {

    }

    public virtual void SendStreamSerializeEvent()
    {

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


public class RiftStream
{
    private List<object> writeData = new List<object>();
    private object[] readData;
    private int currentItem; //Used to track the next item to receive.

    /// <summary>If true, this client should add data to the stream to send it.</summary>
    public bool IsWriting { get; private set; }

    /// <summary>If true, this client should read data send by another client.</summary>
    public bool IsReading
    {
        get { return !this.IsWriting; }
    }

    /// <summary>Count of items in the stream.</summary>
    public int Count
    {
        get { return this.IsWriting ? this.writeData.Count : this.readData.Length; }
    }

    /// <summary>
    /// Creates a stream and initializes it. Used by PUN internally.
    /// </summary>
    public RiftStream(bool write, object[] incomingData)
    {
        this.IsWriting = write;

        if (!write && incomingData != null)
        {
            this.readData = incomingData;
        }
    }

    public void SetReadStream(object[] incomingData, int pos = 0)
    {
        this.readData = incomingData;
        this.currentItem = pos;
        this.IsWriting = false;
    }

    internal void SetWriteStream(List<object> newWriteData, int pos = 0)
    {
        if (pos != newWriteData.Count)
        {
            throw new Exception("SetWriteStream failed, because count does not match position value. pos: " + pos + " newWriteData.Count:" + newWriteData.Count);
        }
        this.writeData = newWriteData;
        this.currentItem = pos;
        this.IsWriting = true;
    }

    internal List<object> GetWriteStream()
    {
        return this.writeData;
    }


    [Obsolete("Either SET the writeData with an empty List or use Clear().")]
    internal void ResetWriteStream()
    {
        this.writeData.Clear();
    }

    /// <summary>Read next piece of data from the stream when IsReading is true.</summary>
    public object ReceiveNext()
    {
        if (this.IsWriting)
        {
            Debug.LogError("Error: you cannot read this stream that you are writing!");
            return null;
        }

        object obj = this.readData[this.currentItem];
        this.currentItem++;
        return obj;
    }

    /// <summary>Read next piece of data from the stream without advancing the "current" item.</summary>
    public object PeekNext()
    {
        if (this.IsWriting)
        {
            Debug.LogError("Error: you cannot read this stream that you are writing!");
            return null;
        }

        object obj = this.readData[this.currentItem];
        //this.currentItem++;
        return obj;
    }

    /// <summary>Add another piece of data to send it when IsWriting is true.</summary>
    public void SendNext(object obj)
    {
        if (!this.IsWriting)
        {
            Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
            return;
        }

        if(obj != null)
        {
            this.writeData.Add(obj);
        }        
    }

    [Obsolete("writeData is a list now. Use and re-use it directly.")]
    public bool CopyToListAndClear(List<object> target)
    {
        if (!this.IsWriting) return false;

        target.AddRange(this.writeData);
        this.writeData.Clear();

        return true;
    }

    /// <summary>Turns the stream into a new object[].</summary>
    public object[] ToArray()
    {
        return this.IsWriting ? this.writeData.ToArray() : this.readData;
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref bool myBool)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(myBool);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                myBool = (bool)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref int myInt)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(myInt);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                myInt = (int)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref string value)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(value);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                value = (string)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref char value)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(value);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                value = (char)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref short value)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(value);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                value = (short)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref float obj)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(obj);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                obj = (float)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref RiftView obj)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(obj);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                obj = (RiftView)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref Vector3 obj)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(obj);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                obj = (Vector3)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref Vector2 obj)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(obj);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                obj = (Vector2)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's IsWriting value.
    /// </summary>
    public void Serialize(ref Quaternion obj)
    {
        if (this.IsWriting)
        {
            this.writeData.Add(obj);
        }
        else
        {
            if (this.readData.Length > this.currentItem)
            {
                obj = (Quaternion)this.readData[this.currentItem];
                this.currentItem++;
            }
        }
    }
}