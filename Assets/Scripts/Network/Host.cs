using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Host : MonoBehaviour
{
    public List<GameObject> playerPrefabs;
    private Dictionary<ulong, int> clientPrefabChoices = new Dictionary<ulong, int>();
    private int selectedPrefabIndex = 0;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        clientPrefabChoices[NetworkManager.Singleton.LocalClientId] = selectedPrefabIndex;
        SpawnHostPlayer();

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Vector3 spawnPosition = GetSpawnPositionForClient(clientId);
            int prefabIndex = clientPrefabChoices.ContainsKey(clientId) ? clientPrefabChoices[clientId] : 0;
            SpawnPlayer(clientId, spawnPosition, prefabIndex);
        };
    }

    public void SelectPrefab(int index)
    {
        if (index >= 0 && index < playerPrefabs.Count)
        {
            selectedPrefabIndex = index;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetClientPrefabChoiceServerRpc(ulong clientId, int prefabIndex)
    {
        if (prefabIndex >= 0 && prefabIndex < playerPrefabs.Count)
        {
            clientPrefabChoices[clientId] = prefabIndex;
        }
    }

    private void SpawnHostPlayer()
    {
        Vector3 spawnPosition = GetSpawnPositionForClient(NetworkManager.Singleton.LocalClientId);
        SpawnPlayer(NetworkManager.Singleton.LocalClientId, spawnPosition, selectedPrefabIndex);
    }

    private void SpawnPlayer(ulong clientId, Vector3 position, int prefabIndex)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        GameObject playerPrefab = playerPrefabs[prefabIndex];
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    private Vector3 GetSpawnPositionForClient(ulong clientId)
    {
        return new Vector3(clientId * 2.0f, 0f, 0f);
    }
}
