using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AvailableHostHandler : MonoBehaviour
{
    [SerializeField] private GameObject availableServerTemplate;
    [SerializeField] private Transform templateParent;

    [SerializeField] private Button refreshServersBtn;
    //[SerializeField] private Button stopRefreshingBtn;

    [SerializeField] private int port = 7777;
    [SerializeField] private string ipAddressPrefix = "192.168.1.";

    private CancellationTokenSource cts = new CancellationTokenSource();
    private CancellationToken cancellationToken;

    private TcpClient client = new TcpClient();
    private System.Net.NetworkInformation.Ping _ping = new System.Net.NetworkInformation.Ping();

    private List<string> availableIPAddresses = new List<string>();

    private bool isSearchingComplated = false;

    void Start()
    {
        cancellationToken = cts.Token;

        refreshServersBtn.onClick.AddListener(CheckServersFromDatabase);


    }

    private void Update()
    {
        if (isSearchingComplated)
        {
            CreateAvailableIPTamplates();
            isSearchingComplated = false;
        }
    }


    private void CheckServersFromDatabase()
    {
        string rawServersData = "";
        StartCoroutine(GetServersRawData());
        
        IEnumerator GetServersRawData()
        {
            using (UnityWebRequest www = UnityWebRequest.Get("https://api.simtechtouch.com/listServers.php"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error sending POST request: " + www.error);
                }
                else
                {
                    Debug.Log("POST request successful!");
                    Debug.Log("Response: " + www.downloadHandler.text);
                    rawServersData = www.downloadHandler.text;
                    ProcessRawData(rawServersData);
                }
            }
        }
    }

    private void ProcessRawData(string rawData)
    {

        if (rawData == "0 results" || rawData == "")
        {
            return;
        }

        string[] ipArray = rawData.Split('&');
        foreach (string ip in ipArray)
        {
            if (!string.IsNullOrEmpty(ip)) {
                availableIPAddresses.Add(ip);
            }
        }
        CreateAvailableIPTamplates();
    }

    private void CreateAvailableIPTamplates()
    {
        // CHANGE THIS LATER WITH A MORE EFFICIENT METHOD
        for (int i = 0; i < templateParent.childCount; i++)
        {
            Destroy(templateParent.GetChild(i).gameObject);
        }

        foreach (string ipAddress in availableIPAddresses)
        {
            var initiatedServer = Instantiate(availableServerTemplate, templateParent);
            initiatedServer.transform.GetChild(2).GetComponent<TMP_Text>().text = ipAddress;

            initiatedServer.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                NetworkManagerController.ConnectSpesifiedServer(ipAddress, (ushort)port);
            });
        }

        refreshServersBtn.GetComponentInChildren<TMP_Text>().text = "Refresh Servers";
        refreshServersBtn.interactable = true;

        availableIPAddresses.Clear();
    }


    //------------------- Pinging all devices is too slow + not exect solution -------------------
    private void CheckNetwork()
    {
        refreshServersBtn.GetComponentInChildren<TMP_Text>().text = "Refreshing...";
        refreshServersBtn.interactable = false;

        CheckAvailableServers().ContinueWith(task =>
        {
            Debug.Log("Task Status: " + task.Status);
            isSearchingComplated |= task.Status == TaskStatus.RanToCompletion;
        });
    }

    private async Task CheckAvailableServers()
    {
        var tasks = new List<Task>();

        // Divide the IP address range into smaller chunks
        int chunkSize = 16;

        for (int i = 1; i < 256; i += chunkSize)
        {
            List<string> ipAddresses = GenerateIPAddressesInRange(i, i + chunkSize);
            tasks.Add(Task.Run(() =>
            {
                foreach (string ipAddress in ipAddresses)
                {
                    Debug.Log("Iteration: " + ipAddress);
                    if (CheckServerAvailabilityAsync(ipAddress))
                    {
                        availableIPAddresses.Add(ipAddress);
                    }
                }

                return Task.CompletedTask;
            }));
        }

        await Task.WhenAll(tasks);
    }

    private List<String> GenerateIPAddressesInRange(int start, int to)
    {
        var ipAddresses = new List<string>();

        for (int i = start; i < to; i++)
        {
            string newIP = ipAddressPrefix + i;
            ipAddresses.Add(newIP);
        }
        return ipAddresses;
    }


    private bool CheckServerAvailabilityAsync(string ip)
    {
        try
        {
            var replyTask = _ping.Send(ip);

            if (replyTask.Status == IPStatus.Success)
            {
                Debug.Log("Ping Success!");
                return true;
            }

            Debug.Log("Failed To Ping!");
            return false;
        }
        catch
        {
            Debug.Log("Failed To Ping!");
            return false;
        }
    }


    //------------------------ TCP Client cannot connect to devices ---------------------
    private void RefreshAvailableServersAsync()
    {
        refreshServersBtn.GetComponentInChildren<TMP_Text>().text = "Refreshing...";
        refreshServersBtn.interactable = false;

        RefreshAvailableServers().ContinueWith(task =>
        {
            refreshServersBtn.GetComponentInChildren<TMP_Text>().text = "Refresh Servers";
            refreshServersBtn.interactable = true;
        });
    }

    private async Task RefreshAvailableServers()
    {
        await Task.Run(() =>
        {
            for (int i = 1; i < 256; i++)
            {
                string ipAddress = ipAddressPrefix + i;
                Debug.Log("Iteration" + ipAddress);
                if (IsServerReachable(ipAddress, port))
                {
                    var initiatedServer = Instantiate(availableServerTemplate, templateParent);
                    initiatedServer.GetComponentInChildren<TMP_Text>().text = ipAddress;

                    initiatedServer.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        NetworkManagerController.ConnectSpesifiedServer(ipAddress, (ushort)port);
                    });
                }
            }
        }, cancellationToken);
    }

    private bool IsServerReachable(string ipAddress, int port)
    {
        try
        {
            client.ReceiveTimeout = 1000;
            client.SendTimeout = 1000;
            client.ReceiveTimeout = 1000;

            client.Connect(ipAddress, port);
            Debug.Log("Connected!");
            client.Close();

            return true;
        }
        catch (Exception)
        {
            Debug.Log("Failed!");
            client.Close();
            return false;
        }
    }
}
