using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> playerPrefabs = new List<GameObject>();
    private Dictionary<ulong, int> playerSelections = new Dictionary<ulong, int>();

    public void SetPlayerPrefab(int prefabIndex)
    {
        if (IsClient)
        {
            SelectPrefabServerRpc(NetworkManager.Singleton.LocalClientId, prefabIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectPrefabServerRpc(ulong clientId, int prefabIndex)
    {
        if (!playerSelections.ContainsKey(clientId))
        {
            playerSelections[clientId] = Mathf.Clamp(prefabIndex, 0, playerPrefabs.Count - 1);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ulong clientId = OwnerClientId;
            SpawnPlayerForClientServerRpc(clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerForClientServerRpc(ulong clientId)
    {
        if (!playerSelections.ContainsKey(clientId) || playerPrefabs.Count == 0)
        {
            return;
        }

        int prefabIndex = playerSelections[clientId];
        GameObject selectedPrefab = playerPrefabs[prefabIndex];

        GameObject playerInstance = Instantiate(selectedPrefab, GetSpawnPosition(), Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
    }
}
