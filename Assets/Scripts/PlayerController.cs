using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Unity.Netcode;
using UnityEditor.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    //속도
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
    private bool isWalk = false;

    //움직임 체크용
    private Vector3 prevPos;
    private float checkTime = 0f;
    //앉기
    [SerializeField]
    private float courchPosY;
    private float originPosY;
    private float applyCrouchPosY;


    private CapsuleCollider capsuleCollider;

    //민감도
    [SerializeField]
    private float lookSensitivity;

    //카메라
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f;

    //컴포넌트
    [SerializeField]
    private Camera mainCam;
    private Rigidbody rb;
    private Transform mainCamTransform;
    private GunController gunController;
    private CrossHair crossHair;

    private GameObject Canvas;
    private GameObject hud;


    NetworkVariable<int> randomValue = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        gunController = FindObjectOfType<GunController>();
        crossHair = FindObjectOfType<CrossHair>();

        Canvas = GameObject.Find("Canvas");
        hud = Canvas.transform.Find("HUD").gameObject;
        hud.SetActive(true);
        hud.GetComponent<HUD>().gunController = gunController;
        crossHair.gunController = gunController;

        mainCamTransform = mainCam.transform;
        originPosY = mainCam.transform.localPosition.y;
        applyCrouchPosY = originPosY;
        applySpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            randomValue.Value = Random.Range(0, 100);
            Debug.Log(OwnerClientId + "; random value: " + randomValue.Value);

        }

        IsGround();

        TryJump();
        TryRun();
        TryCrouch();

        Move();
        MoveCheck();

        CameraRotation();
        CharacterRotation();
    }

    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        isCrouch = !isCrouch;
        crossHair.CrouchingAnimation(isCrouch);

        if (IsOwner)
            SendCrouchServerRpc(new ServerRpcParams());
        if (isCrouch)
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
        float posY = mainCamTransform.localPosition.y;
        int count = 0;
        while (posY != applyCrouchPosY)
        {
            count++;
            posY = Mathf.Lerp(posY, applyCrouchPosY, 0.3f);
            mainCamTransform.localPosition = new Vector3(0, posY, 0);
            if (count > 15)
                break;
            yield return null;
        }
        mainCamTransform.localPosition = new Vector3(0, applyCrouchPosY, 0);

    }
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        crossHair.JumpingAnimation(!isGround);
    }

    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (isCrouch)
            Crouch();

        rb.velocity = transform.up * jumpForce;
    }

    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    private void Running()
    {
        if (isCrouch)
            Crouch();
        gunController.CancelFineSight();

        isRun = true;
        crossHair.RunningAnimation(isRun);
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        crossHair.RunningAnimation(isRun);
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

    private void MoveCheck()
    {
        checkTime += Time.deltaTime;
        if (!isRun && !isCrouch && isGround)
        {
            if (checkTime > 0.1f)
            {
                if (Vector3.Distance(prevPos, transform.position) >= 0.01f)
                {
                    isWalk = true;
                    Debug.Log(Vector3.Distance(prevPos, transform.position));
                }
                else
                    isWalk = false;
                crossHair.WalkingAnimation(isWalk);
                prevPos = transform.position;
                checkTime = 0f;
            }
        }
    }
    private void CameraRotation()
    {
        float rotationX = Input.GetAxisRaw("Mouse Y");
        float cameraRotationX = rotationX * lookSensitivity;
        currentCameraRotationX -= cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

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
        Cursor.lockState = CursorLockMode.Locked;
        if (!IsOwner)
        {
            foreach (var cam in GetComponentsInChildren<Camera>())
            {
                Destroy(cam);
                //Debug.Log("Destory "+cam.name);
            }
            Destroy(mainCam.GetComponent<AudioListener>());
        }
    }
    [ClientRpc]
    void SendCrouchClientRpc(ulong sendId)
    {
        if (OwnerClientId == sendId && !IsOwner)
        {
            Crouch();
        }
    }

    [ServerRpc]
    void SendCrouchServerRpc(ServerRpcParams serverParams)
    {
        if (IsHost && OwnerClientId != serverParams.Receive.SenderClientId)
        {
            //Debug.Log("I must crouch!");
        }
        SendCrouchClientRpc(serverParams.Receive.SenderClientId);
    }
}
