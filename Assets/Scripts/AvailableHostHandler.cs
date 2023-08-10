using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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

    void Start()
    {
        cancellationToken = cts.Token;

        refreshServersBtn.onClick.AddListener(RefreshAvailableServersAsync);

        if (IsServerReachable("127.0.0.1", port))
        {
            var initiatedServer = Instantiate(availableServerTemplate, templateParent);
            initiatedServer.GetComponentInChildren<TMP_Text>().text = "127.0.0.1";

            initiatedServer.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                NetworkManagerController.ConnectSpesifiedServer("127.0.0.1", (ushort)port);
            });
        }
    }

    void Update()
    {

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
