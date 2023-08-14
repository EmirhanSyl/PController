using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;

public class ServerBehaviours : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += WhenServerStarted;
        NetworkManager.Singleton.OnServerStopped += WhenServerStopped;
    }

    private void WhenServerStarted()
    {
        Debug.Log("Server Started");
        string ip = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address;
        StartCoroutine(ServerStartedPostRequest(ip, "Server Name"));
    }

    private void WhenServerStopped(bool isStopped)
    {
        string ip = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address;

        WWWForm form = new WWWForm();
        form.AddField("ip_address", ip);

        UnityWebRequest www = UnityWebRequest.Post("https://api.simtechtouch.com/removeServer.php", form);
        www.SendWebRequest().completed += (op) => {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error sending POST request: " + www.error);
            }
            else
            {
                Debug.Log("POST request successful!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }

            www.Dispose();
        };

        

        Debug.Log("Server Stopped!");
    }

    private IEnumerator ServerStartedPostRequest(string ipAddress, string serverName)
    {
        WWWForm form = new WWWForm();
        form.AddField("ip_address", ipAddress);
        form.AddField("server_name", serverName);

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.simtechtouch.com/createServer.php", form))
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
            }
        }
    }

    
}
