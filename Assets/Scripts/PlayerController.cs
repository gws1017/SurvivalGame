using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Unity.Netcode;
using UnityEditor.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    //¼Óµµ
    [SerializeField]
    private float walkSpeed;
    [SerializeField] 
    private float runSpeed;
    private float applySpeed;
    [SerializeField]
    private float crouchSpeed;

    [SerializeField]
    private float jumpForce;

    //bool
    private bool isRun = false;
    private bool isGround = true;
    private bool isCrouch = false;

    //¾É±â
    [SerializeField]
    private float courchPosY;
    private float originPosY;
    private float applyCrouchPosY;


    private CapsuleCollider capsuleCollider;

    //¹Î°¨µµ
    [SerializeField]
    private float lookSensitivity;

    //Ä«¸Þ¶ó
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f;

    //ÄÄÆ÷³ÍÆ®
    [SerializeField]
    private Camera mainCam;

    private Rigidbody rb;

    NetworkVariable<int> randomValue = new NetworkVariable<int>(1,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        applySpeed = walkSpeed;
        originPosY = mainCam.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if(Input.GetKeyDown(KeyCode.T))
        {
            randomValue.Value = Random.Range(0, 100);
            Debug.Log(OwnerClientId + "; random value: " + randomValue.Value);

        }
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
    }

    private void TryCrouch()
    {
        if(Input.GetKeyDown (KeyCode.LeftControl)) 
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        isCrouch = !isCrouch;
        if(isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = courchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }
        StartCoroutine(CrouchCorutine());
    }

    IEnumerator CrouchCorutine()
    {
        float posY = mainCam.transform.localPosition.y;
        int count = 0;
        while(posY != applyCrouchPosY)
        {
            count++;
            posY = Mathf.Lerp(posY, applyCrouchPosY,0.3f);
            mainCam.transform.localPosition = new Vector3(0, posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        mainCam.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);

    }
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    private void TryJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround) 
        {
            Jump();
        }
    }

    private void Jump()
    {
        if(isCrouch)
            Crouch();

        rb.velocity = transform.up * jumpForce;
    }

    private void TryRun()
    {
        if(Input.GetKey(KeyCode.LeftShift)) 
        {
            Running();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    private void Running()
    {
        if (isCrouch)
            Crouch();

        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }
    private void Move()
    {
        float moveDX = Input.GetAxisRaw("Horizontal");
        float moveDZ = Input.GetAxisRaw("Vertical");

        Vector3 moveHorizontal = transform.right * moveDX;
        Vector3 moveVertical = transform.forward * moveDZ;
        Vector3 velocity = (moveHorizontal + moveVertical).normalized * applySpeed;

        //transform.position += (velocity * Time.deltaTime);

        rb.MovePosition(transform.position + velocity * Time.deltaTime);
    }

    private void CameraRotation()
    {
        float rotationX = Input.GetAxisRaw("Mouse Y");
        float cameraRotationX = rotationX * lookSensitivity;
        currentCameraRotationX -= cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX,-cameraRotationLimit,cameraRotationLimit);

        mainCam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    private void CharacterRotation()
    {
        float rotationY = Input.GetAxisRaw("Mouse X");
        Vector3 characterRotationY = new Vector3(0f, rotationY, 0f) * lookSensitivity;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(characterRotationY));
    }

    public override void OnNetworkSpawn()
    {
        Cursor.visible = false;
        if (!IsOwner)
        {
            Destroy(mainCam);
        }
    }

    [ClientRpc]
    void TestClientRpc(int value, ulong sourceNetworkObjectId)
    {
        Debug.Log($"Client Received the RPC #{value} on Netwrok Object #{sourceNetworkObjectId}");
        if(IsOwner)
        {
            TestServerRpc(value + 1, sourceNetworkObjectId);
        }
    }

    [ServerRpc]    
    void TestServerRpc(int value, ulong sourceNetworkObjectId)
    {
        Debug.Log($"Server Received the RPC #{value}, on NetworkObject #{sourceNetworkObjectId}");
        TestClientRpc(value, sourceNetworkObjectId);
    }
}
