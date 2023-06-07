using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRiftSerializable
{
    public void OnStreamSerializeEvent(RiftStream Stream);
    public void OnStreamDeserializeEvent(RiftStream Stream);
}
