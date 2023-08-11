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
    private PingReply _reply;

    void Start()
    {
        cancellationToken = cts.Token;

        refreshServersBtn.onClick.AddListener(CheckNetwork);

        //CheckServerAvailability("192.168.1.200");
    }

    void Update()
    {

    }

    private void CheckNetwork()
    {
        CheckAvailableServers();
    }
    private async Task CheckAvailableServers()
    {
        var tasks = new List<Task>();

        // Divide the IP address range into smaller chunks
        int chunkSize = 16; 

        for (int i = 1; i < 256; i += chunkSize)
        {
            List<string> ipAddresses = GenerateIPAddressesInRange(i, i + chunkSize);
            tasks.Add(Task.Run(async () =>
            {
                foreach (string ipAddress in ipAddresses)
                {
                    Debug.Log("Iteration: " + ipAddress);
                    if (await CheckServerAvailabilityAsync(ipAddress))
                    {
                        var initiatedServer = Instantiate(availableServerTemplate, templateParent);
                        initiatedServer.GetComponentInChildren<TMP_Text>().text = ipAddress;

                        initiatedServer.GetComponentInChildren<Button>().onClick.AddListener(() =>
                        {
                            NetworkManagerController.ConnectSpesifiedServer(ipAddress, (ushort)port);
                        });
                    }
                }
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


    private async Task<bool> CheckServerAvailabilityAsync(string ip)
    {
        try
        {
            var replyTask = Task.Run(() => _ping.Send(ip));
            await Task.WhenAny(replyTask, Task.Delay(200));

            if (replyTask.IsCompleted && replyTask.Result.Status == IPStatus.Success)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

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
