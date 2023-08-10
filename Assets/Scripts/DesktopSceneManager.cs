using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DesktopSceneManager : MonoBehaviour
{
    [SerializeField] private TMP_Text connectedCountText;

    void Start()
    {
        
    }

    void Update()
    {
        if (!NetworkManager.Singleton.IsHost && NetworkManager.Singleton.IsClient)
        {
            SceneManager.LoadScene(2);
        }

        connectedCountText.text = "Connected: " + NetworkManager.Singleton.ConnectedClientsList.Count;
    }

    
}
