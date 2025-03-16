using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Host : NetworkBehaviour
{
    public List<GameObject> playerPrefabs;
    private Dictionary<ulong, int> clientPrefabChoices = new Dictionary<ulong, int>();
    private HashSet<ulong> spawnedPlayers = new HashSet<ulong>();
    private int selectedPrefabIndex = 0;

    public void StartHost()
    {
        Debug.Log("Startar host...");
        NetworkManager.Singleton.StartHost();
        ulong hostClientId = NetworkManager.Singleton.LocalClientId;
        clientPrefabChoices[hostClientId] = selectedPrefabIndex;
        spawnedPlayers.Add(hostClientId);
        Debug.Log($"Värdens clientID: {hostClientId} med prefab-index: {selectedPrefabIndex}");
        SpawnHostPlayer();
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId == hostClientId)
                return;
            Debug.Log($"Klient ansluten: {clientId}");
        };
    }

    public void SelectPrefab(int index)
    {
        if (index >= 0 && index < playerPrefabs.Count)
        {
            selectedPrefabIndex = index;
            Debug.Log($"Prefab valts: {index}");
        }
        else
        {
            Debug.LogWarning($"Ogiltigt prefab-index: {index}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetClientPrefabChoiceServerRpc(int prefabIndex, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        if (senderClientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Host skickade prefab-val, ignorerar RPC.");
            return;
        }
        if (prefabIndex < 0 || prefabIndex >= playerPrefabs.Count)
        {
            Debug.LogWarning($"Client {senderClientId} skickade ogiltigt prefab-index: {prefabIndex}");
            return;
        }
        clientPrefabChoices[senderClientId] = prefabIndex;
        Debug.Log($"Client {senderClientId} valde prefab-index: {prefabIndex}");
        if (!spawnedPlayers.Contains(senderClientId))
        {
            Vector3 spawnPosition = GetSpawnPositionForClient(senderClientId);
            Debug.Log($"Spawning client: {senderClientId} med prefab-index: {prefabIndex} vid position: {spawnPosition}");
            SpawnPlayer(senderClientId, spawnPosition, prefabIndex);
            spawnedPlayers.Add(senderClientId);
        }
        else
        {
            Debug.LogWarning($"Client {senderClientId} är redan spawnad!");
        }
    }

    private void SpawnHostPlayer()
    {
        Vector3 spawnPosition = GetSpawnPositionForClient(NetworkManager.Singleton.LocalClientId);
        Debug.Log($"Spawnar värdens spelare vid position: {spawnPosition} med prefab-index: {selectedPrefabIndex}");
        SpawnPlayer(NetworkManager.Singleton.LocalClientId, spawnPosition, selectedPrefabIndex);
    }

    private void SpawnPlayer(ulong clientId, Vector3 position, int prefabIndex)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("SpawnPlayer anropat på klient-sidan, vilket inte är tillåtet.");
            return;
        }
        GameObject playerPrefab = playerPrefabs[prefabIndex];
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        Debug.Log($"Instansierar spelare för client {clientId} med prefab {playerPrefab.name} på position {position}");
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    private Vector3 GetSpawnPositionForClient(ulong clientId)
    {
        Vector3 position = new Vector3(clientId * 2.0f, 0f, 0f);
        Debug.Log($"Beräknad spawn-position för client {clientId}: {position}");
        return position;
    }
}
