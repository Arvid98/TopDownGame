using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    public NetworkObject cockModel;
    public NetworkObject builderModel;
    public NetworkObject blackSmithModel;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "Game" || !NetworkManager.Singleton.IsServer)
            return;

        var lobby = LobbyManager.Instance.GetJoinedLobby();
        var players = lobby.Players;




        for (int i = 0; i < clientsCompleted.Count; i++)
        {
            ulong clientId = clientsCompleted[i];
            var p = players[i];

            if (!System.Enum.TryParse(
                    p.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value,
                    out LobbyManager.PlayerCharacter c))
                c = LobbyManager.PlayerCharacter.Cock;

            var prefab = c switch
            {
                LobbyManager.PlayerCharacter.Cock => cockModel,
                LobbyManager.PlayerCharacter.Builder => builderModel,
                LobbyManager.PlayerCharacter.BlackSmith => blackSmithModel,
                _ => cockModel
            };

            Debug.Log($"[Spawner] Spawnar {c} för client {clientId}");
            var instance = Instantiate(prefab);
            instance.SpawnAsPlayerObject(clientId, true);
        }
    }

}
