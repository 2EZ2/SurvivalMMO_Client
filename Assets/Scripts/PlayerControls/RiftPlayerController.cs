using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftPlayerController : RiftBehaviour
{

    public CharacterController m_CharacterController;

    private Vector3 m_MoveDir = Vector3.zero;

    [SerializeField]
    private int health = 100;

    [SyncVar]
    public int Health { get => health; set => health = value; }


    // Start is called before the first frame update
    void Start()
    {
        Identity = GetComponent<RiftIdentity>();
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


    public void ShootGun()
    {       
        Identity.View.RPC(this.GetType().Name, "ShootGunRPC", RPCTarget.Everyone, 10, "was Mad");
    }


    [RiftRPC]
    public void ShootGunRPC(int damage, string reason)
    {
        Debug.Log($@"Remote Gun Shot did {damage} because {reason}");
    }
}
