using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float walkSpeed;

    [SerializeField]
    private float lookSensitivity;

    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f;

    [SerializeField]
    private Camera mainCam;

    private Rigidbody rb;

    NetworkVariable<int> randomValue = new NetworkVariable<int>(1,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(OwnerClientId + "; random value: " + randomValue.Value);
        if (!IsOwner) return;
        if(Input.GetKeyDown(KeyCode.T))
        {
            randomValue.Value = Random.Range(0, 100);
        }
        Move();
        CameraRotation();
        CharacterRotation();
    }

    private void Move()
    {
        float moveDX = Input.GetAxisRaw("Horizontal");
        float moveDZ = Input.GetAxisRaw("Vertical");

        Vector3 moveHorizontal = transform.right * moveDX;
        Vector3 moveVertical = transform.forward * moveDZ;
        Vector3 velocity = (moveHorizontal + moveVertical).normalized * walkSpeed;

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
       if(!IsServer && IsOwner)
        {
            
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
