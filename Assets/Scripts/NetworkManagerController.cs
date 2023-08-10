using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerController : MonoBehaviour
{
    [SerializeField] private Button startHostBtn;
    [SerializeField] private Button startServerBtn;
    [SerializeField] private Button startClientBtn;

    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_InputField ipField;

    void Awake()
    {

        startHostBtn.onClick.AddListener(() =>
        {
            SetConnection();
            NetworkManager.Singleton.StartHost();
        });

        startServerBtn.onClick.AddListener(() =>
        {
            SetConnection();
            NetworkManager.Singleton.StartServer();
        });

        startClientBtn.onClick.AddListener(() =>
        {
            SetConnection();
            NetworkManager.Singleton.StartClient();
        });
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            {
                Debug.Log("CLIENT CONNECTED! Client ID: " + id);
            };
        }

    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClientsList.Count > 1)
        {
            StartCoroutine(LoadSceneCorutine(1));
        }
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            StartCoroutine(LoadSceneCorutine(2));
        }

    }

    IEnumerator LoadSceneCorutine(int sceneIndex)
    {
        statusText.text = "Connection established! Launching App...";
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(sceneIndex);
    }

    private void SetConnection()
    {
        string ip = ipField.text;
        if (!string.IsNullOrEmpty(ip))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                ip,
                7777,
                "0.0.0.0"
            );
        }
    }
}
