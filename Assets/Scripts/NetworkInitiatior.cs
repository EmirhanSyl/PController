using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkInitiatior : NetworkBehaviour
{
    public GameObject controllerPrefab;
    private GameObject controllerPrefabInstance;
    private NetworkObject spawnedNetworkObject;

    public override void OnNetworkSpawn()
    {
        enabled = IsServer; 
        if (!enabled || controllerPrefab == null)
        {
            return;
        }

        controllerPrefabInstance = Instantiate(controllerPrefab);

        spawnedNetworkObject = controllerPrefabInstance.GetComponent<NetworkObject>();
        spawnedNetworkObject.Spawn(false);
    }


}
