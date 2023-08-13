using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class NetworkManagerController : MonoBehaviour
{
    [SerializeField] private Button startHostBtn;
    [SerializeField] private Button startServerBtn;
    [SerializeField] private Button startClientBtn;

    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_InputField ipField;

    void Awake()
    {

        startHostBtn.onClick.AddListener(() =>
        {
            SetConnection(GetDeviceIP(), 7777);
            NetworkManager.Singleton.StartHost();
            var connData = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData;
            statusText.text = "Server Created. Server ip: " + connData.Address + " Server port: " + connData.Port;
        });

        startServerBtn.onClick.AddListener(() =>
        {
            SetConnection(GetDeviceIP(), 7777);
            NetworkManager.Singleton.StartServer();
            var connData = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData;
            statusText.text = "Server Created. Server ip: " + connData.Address + " Server port: " + connData.Port;
        });

        startClientBtn.onClick.AddListener(() =>
        {
            SetConnection(ipField.text, 7777);
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

    #region PING
    //IEnumerator StartPing(string ip)
    //{
    //    WaitForSeconds f = new WaitForSeconds(0.05f);
    //    Ping p = new Ping(ip);
    //    while (p.isDone == false)
    //    {
    //        yield return f;
    //    }
    //    PingFinished(p);
    //}


    //public void PingFinished(Ping p)
    //{
    //    Debug.Log("Ping Done for ip: " + p.ip + " time: " + p.time);
    //}
    #endregion

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
        headerText.text = "Connection established! Launching App...";
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(sceneIndex);
    }

    private static void SetConnection(string ip, int port)
    {
        if (!string.IsNullOrEmpty(ip))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                ip,
                (ushort)port,
                "0.0.0.0"
            );
        }
    }


    public static void ConnectSpesifiedServer(string ip, ushort port)
    {
        if (!string.IsNullOrEmpty(ip))
        {
            SetConnection(ip, port);

            NetworkManager.Singleton.StartClient();
        }
    }

    private string GetDeviceIP()
    {
        string ipAddress = "";

        // Get all available network interfaces
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface networkInterface in networkInterfaces)
        {
            // Ignore loopback and non-operational interfaces
            if (networkInterface.OperationalStatus != OperationalStatus.Up || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            // Get IP properties for the network interface
            IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

            // Get the IPv4 addresses associated with the network interface
            foreach (UnicastIPAddressInformation ipInfo in ipProperties.UnicastAddresses)
            {
                if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ipInfo.Address.ToString();
                    break;
                }
            }

        }

        return ipAddress;
    }
}
