using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;

public class ServerHeartbeat : MonoBehaviour
{
    private static ServerHeartbeat instance;
    private float interval = 30f; // 30 seconds
    private float nextActionTime;

    private string ip = "";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        nextActionTime = Time.time + interval;
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer) { return; }

        if (ip.Equals("") || ip == null)
        {
            ip = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address;
            Debug.Log("Ip adjusted!");
            return;
        }

        if (Time.time > nextActionTime)
        {
            nextActionTime += interval;

            StartCoroutine(SendHeartbeat(ip));
            Debug.Log("DýpDýp");
        }
    }

    private IEnumerator SendHeartbeat(string ipAddress)
    {
        WWWForm form = new WWWForm();
        form.AddField("ip_address", ipAddress);

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.simtechtouch.com/heartbeatServer.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error sending POST request: " + www.error);
            }
            else
            {
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
}
