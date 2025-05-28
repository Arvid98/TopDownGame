using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn
{
    private GameObject selectedPrefab;

    public PlayerSpawn(GameObject prefab)
    {
        selectedPrefab = prefab;
    }

    public void SpawnPlayer(ulong clientId, Vector3 position)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        GameObject player = Object.Instantiate(selectedPrefab, position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        Debug.Log($"Spawned player for client {clientId} at position {position}");
    }
}
