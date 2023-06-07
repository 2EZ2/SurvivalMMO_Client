using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftStream 
{
    private List<object> writeData = new List<object>();
    private object[] readData;
    private int currentItem; //Used to track the next item to receive.

    public bool IsWriting { get; set; }

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
        else if(write && incomingData != null)
        {
            writeData.AddRange(incomingData);
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

        if (obj != null)
        {
            this.writeData.Add(obj);
        }
    }

    /// <summary>Turns the stream into a new object[].</summary>
    public object[] ToArray()
    {
        return this.IsWriting ? this.writeData.ToArray() : this.readData;
    }
}

