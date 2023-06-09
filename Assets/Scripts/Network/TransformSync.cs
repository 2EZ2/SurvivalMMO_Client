using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSync : RiftBehaviour, IRiftSerializable
{
    /// <summary>
    ///     The position to lerp to.
    /// </summary>
    public Vector3 NewPosition { get; set; }

    /// <summary>
    ///     The rotation to lerp to.
    /// </summary>    
    public Vector3 NewRotation { get; set; }

    /// <summary>
    ///     The speed to lerp the player's position.
    /// </summary>
    [SerializeField]
    [Tooltip("The speed to lerp position")]
    public float moveLerpSpeed = 10f;

    /// <summary>
    ///     The speed to lerp the player's rotation.
    /// </summary>
    [SerializeField]
    [Tooltip("The speed to lerp rotation")]
    public float rotateLerpSpeed = 50f;


    public void OnStreamDeserializeEvent(RiftStream Stream)
    {
        NewPosition = (Vector3)((vec3)Stream.ReceiveNext());
        NewRotation = (Vector3)((vec3)Stream.ReceiveNext());
    }

    public void OnStreamSerializeEvent(RiftStream Stream)
    {
        Stream.SendNext(new vec3(transform.position));
        Stream.SendNext(new vec3(transform.rotation.eulerAngles));
    }

    public void RemoteUpdateTransform()
    {
        //Move and rotate to new values
        transform.position = Vector3.Lerp(transform.position, NewPosition, Time.deltaTime * moveLerpSpeed);
        transform.eulerAngles = new Vector3(
            Mathf.LerpAngle(transform.eulerAngles.x, NewRotation.x, Time.deltaTime * rotateLerpSpeed),
            Mathf.LerpAngle(transform.eulerAngles.y, NewRotation.y, Time.deltaTime * rotateLerpSpeed),
            Mathf.LerpAngle(transform.eulerAngles.z, NewRotation.z, Time.deltaTime * rotateLerpSpeed)
        );
    }

    // Start is called before the first frame update
    void Start()
    {
        Identity = GetComponent<RiftIdentity>();
        NewPosition = transform.position;
        NewRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsMine)
        {
            RemoteUpdateTransform();
        }
    }
}
