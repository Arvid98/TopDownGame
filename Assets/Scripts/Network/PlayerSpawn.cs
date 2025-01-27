using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner
{
    private GameObject playerPrefab;

    public PlayerSpawner(GameObject prefab)
    {
        playerPrefab = prefab;
    }

    public void SpawnPlayer(ulong clientId, Vector3 position)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("Only the server can spawn players.");
            return;
        }

        GameObject player = Object.Instantiate(playerPrefab, position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        Debug.Log($"Spawned player for client {clientId} at position {position}");
    }
    //private void Start()
    //{
    //    if (NetworkManager.Singleton != null)
    //    {
    //        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    //    }
    //}

    //private void OnDestroy()
    //{
    //    if (NetworkManager.Singleton != null)
    //    {
    //        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    //    }
    //}

}
