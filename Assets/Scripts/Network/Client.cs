using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Client : MonoBehaviour
{
    public Host host;
    public List<GameObject> playerPrefabs;
    private int selectedPrefabIndex = 0;

    public void SelectPrefab(int index)
    {
        if (index >= 0 && index < playerPrefabs.Count)
        {
            selectedPrefabIndex = index;
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SendPrefabChoiceToServer();
            }
        };
    }

    private void SendPrefabChoiceToServer()
    {
        if (host != null)
        {
            host.SetClientPrefabChoiceServerRpc(NetworkManager.Singleton.LocalClientId, selectedPrefabIndex);
        }
    }
}
