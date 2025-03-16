using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
    public Host host;
    public List<GameObject> playerPrefabs;
    private int selectedPrefabIndex = 0;
    private bool prefabChoiceSent = false;

    public void SelectPrefab(int index)
    {
        if (index >= 0 && index < playerPrefabs.Count)
        {
            selectedPrefabIndex = index;
            Debug.Log($"Client: Prefab valts: {index}");
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId && !NetworkManager.Singleton.IsHost && !prefabChoiceSent)
            {
                SendPrefabChoiceToServer();
                prefabChoiceSent = true;
            }
        };
    }

    private void SendPrefabChoiceToServer()
    {
        if (host != null)
            host.SetClientPrefabChoiceServerRpc(selectedPrefabIndex);
        else
            Debug.LogWarning("Host-referensen saknas!");
    }
}
