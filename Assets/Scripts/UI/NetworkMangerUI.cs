using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMangerUI : NetworkBehaviour
{
    [SerializeField] Button serverBtn;
    [SerializeField] Button hostBtn;
    [SerializeField] Button clientBtn;
    // Start is called before the first frame update
    void Awake()
    {
        serverBtn.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        hostBtn.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        clientBtn.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
    }
}
