using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftPlayerController : RiftBehaviour, IRiftSerializable
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
    public Vector3 NewPosition { get; set; }

    /// <summary>
    ///     The rotation to lerp to.
    /// </summary>    
    public Vector3 NewRotation { get; set; }


    [RiftSyncVar]
    public int Health { get => health; set => health = value; }


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

    
    private int health = 100;

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
            if (Input.GetKeyDown(KeyCode.E))
            {
                ShootGun();
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
            m_MoveDir.x = desiredMove.x * 50f;
            m_MoveDir.z = desiredMove.z * 50f;

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

    public void ShootGun()
    {       
        _RiftView.RPC("ShootGunRPC", RPCTarget.Everyone, 10, "was Mad");
    }

    [RiftRPC]
    public void ShootGunRPC(int damage, string reason)
    {
        Debug.Log($@"Remote Gun Shot did {damage} because {reason}");
    }

    public void OnStreamSerializeEvent(RiftStream inStream)
    {
        inStream.SendNext(new vec3(transform.position));
        inStream.SendNext(new vec3(transform.rotation.eulerAngles));

        lastPosition = transform.position;
        lastRotation = transform.rotation.eulerAngles;
    }

    public void OnStreamDeserializeEvent(RiftStream stream)
    {
        NewPosition = (Vector3)((vec3)stream.ReceiveNext());
        NewRotation = (Vector3)((vec3)stream.ReceiveNext());
    }
}
