using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkMessageHandler : NetworkBehaviour
{
    [SerializeField] private Button testRPCButton;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            testRPCButton = GameObject.FindGameObjectWithTag("RPCMessageSender").GetComponent<Button>();                
            testRPCButton.onClick.AddListener(() => SendMessageServerRPC("Button Clicked!", new ServerRpcParams()));
        }
        
    }

    void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SendMessageServerRPC("T Key Pressed", new ServerRpcParams());
        }
    }


    [ServerRpc]
    private void SendMessageServerRPC(string message, ServerRpcParams p)
    {
        Debug.Log("Message: " + message);
    }

}
