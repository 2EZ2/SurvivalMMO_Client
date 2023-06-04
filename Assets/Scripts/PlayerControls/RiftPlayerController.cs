using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftPlayerController : RiftBehaviour
{
    /// <summary>
    ///     The speed to lerp the player's position.
    /// </summary>
    [SerializeField]
    [Tooltip("The speed to lerp the player's position")]
    public float moveLerpSpeed = 10f;

    /// <summary>
    ///     The speed to lerp the player's rotation.
    /// </summary>
    [SerializeField]
    [Tooltip("The speed to lerp the player's rotation")]
    public float rotateLerpSpeed = 50f;

    /// <summary>
    ///     The position to lerp to.
    /// </summary>
    [RiftSyncVar]
    public Vector3 NewPosition { get; set; }

    /// <summary>
    ///     The rotation to lerp to.
    /// </summary>
    [RiftSyncVar]
    public Vector3 NewRotation { get; set; }


    /// <summary>
    ///     The last position our character was at.
    /// </summary>
    Vector3 lastPosition;

    /// <summary>
    ///     The last rotation our character was at.
    /// </summary>
    Vector3 lastRotation;
    public CharacterController m_CharacterController;
    private Vector3 m_MoveDir = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //Set initial values
        NewPosition = transform.position;
        NewRotation = transform.eulerAngles;
        lastPosition= transform.position;
        lastRotation = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMine)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");


            if (Vector3.SqrMagnitude(transform.position - lastPosition) > 0.1f ||
                Vector3.SqrMagnitude(transform.eulerAngles - lastRotation) > 5f) 
            { 
                SendStreamSerializeEvent(); 
            }           
        }
        else
        {
            RemoteUpdateTransform();
        }
    }
    private void FixedUpdate()
    {
        if (IsMine)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 desiredMove = transform.forward * vertical + transform.right * horizontal;
            m_MoveDir.x = desiredMove.x * 100f;
            m_MoveDir.z = desiredMove.z * 100f;

            m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        }
            
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

    public override void SendStreamSerializeEvent()
    {
        RiftStream stream = new RiftStream(true, null);      
        stream.SendNext(new vec3(transform.position));
        stream.SendNext(new vec3(transform.rotation.x, transform.rotation.y, transform.rotation.z));
        RiftManager.SendSync(this._RiftView, stream);

        lastPosition = transform.position;
        lastRotation = new vec3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
    }

    public override void OnStreamSerializeEvent(RiftStream stream)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(new vec3(transform.position));
            stream.SendNext(new vec3(transform.rotation.x, transform.rotation.y, transform.rotation.z));
        }
    }

    public override void OnStreamDeserializeEvent(RiftStream stream)
    {
        Debug.Log("Deserializing player movement stream");
        NewPosition = (Vector3)((vec3)stream.ReceiveNext());
        NewRotation = (Vector3)((vec3)stream.ReceiveNext());
    }
}
