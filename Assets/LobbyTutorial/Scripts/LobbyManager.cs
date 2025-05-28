using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_CHARACTER = "Character";
    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_COUNTDOWN_TIMER = "CountdownTimer";

    public event EventHandler OnLeftLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;



    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public enum GameMode { Creative, Survivel }
    public enum PlayerCharacter { Cock, Builder, BlackSmith }

    public PlayerCharacter selectedCharacter { get; private set; } = PlayerCharacter.Cock;

    private Lobby joinedLobby;
    private string playerName;
    private bool relayClientStarted = false;

    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyListTimer = 5f;
    private float gameCountdownTimer = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = false;

    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async void Authenticate(string playerName)
    {
        this.playerName = playerName;
        var initOptions = new InitializationOptions().SetProfile(playerName);
        await UnityServices.InitializeAsync(initOptions);
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
            RefreshLobbyList();
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 15f;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = 1.1f;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                if (!IsPlayerInLobby())
                {
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                    joinedLobby = null;
                }
            }
        }
    }

    public Lobby GetJoinedLobby() => joinedLobby;
    public bool IsLobbyHost() => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    private bool IsPlayerInLobby()
    {
        if (joinedLobby?.Players != null)
            foreach (var p in joinedLobby.Players)
                if (p.Id == AuthenticationService.Instance.PlayerId) return true;
        return false;
    }

    private Player GetPlayer() =>
        new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>
        {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerCharacter.Cock.ToString()) }
        });

    public void ChangeGameMode()
    {
        if (!IsLobbyHost()) return;
        var mode = Enum.Parse<GameMode>(joinedLobby.Data[KEY_GAME_MODE].Value);
        mode = mode == GameMode.Creative ? GameMode.Survivel : GameMode.Creative;
        UpdateLobbyGameMode(mode);
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, GameMode gameMode)
    {
        joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions
        {
            Player = GetPlayer(),
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
            {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
            }
        });
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        await SetupRelayAndStartHost();
    }

    public async void RefreshLobbyList()
    {
        try
        {
            var results = await LobbyService.Instance.QueryLobbiesAsync();
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = results.Results });
        }
        catch { }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions { Player = GetPlayer() });
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        JoinRelayAndStartClient();
    }

    public async void JoinLobby(Lobby lobbyToJoin)
    {
        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyToJoin.Id, new JoinLobbyByIdOptions { Player = GetPlayer() });
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        JoinRelayAndStartClient();
    }

    public async void QuickJoinLobby()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions());
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            JoinRelayAndStartClient();
        }
        catch { }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby == null) return;
        await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        joinedLobby = null;
        if (relayClientStarted) return;
        relayClientStarted = false;
        OnLeftLobby?.Invoke(this, EventArgs.Empty);
    }

    public async void KickPlayer(string playerId)
    {
        if (!IsLobbyHost()) return;
        await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
    }

    public async void UpdatePlayerName(string newName)
    {
        playerName = newName;
        if (joinedLobby == null) return;
        joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, newName) }
            }
        });
        OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
    }

    public async void UpdatePlayerCharacter(PlayerCharacter character)
    {
        selectedCharacter = character;
        if (joinedLobby == null) return;
        joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, character.ToString()) }
            }
        });
        OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
    }

    public async void UpdateLobbyGameMode(GameMode gameMode)
    {
        joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) }
            }
        });
        OnLobbyGameModeChanged?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
    }

    private async Task SetupRelayAndStartHost()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(4);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
            }
        });
        var relayData = AllocationUtils.ToRelayServerData(allocation, RelayServerEndpoint.ConnectionTypeDtls);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
        NetworkManager.Singleton.StartHost();
    }

    public void StartGame()
    {
        if (!IsLobbyHost()) return;
        //if (NetworkManager.Singleton.ConnectedClientsList.Count <= 1) return;


        //NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);

        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);

        
    }

    public async void JoinRelayAndStartClient()
    {
        if (relayClientStarted) return;
        relayClientStarted = true;

        float timeout = 20f;
        while ((!joinedLobby.Data.ContainsKey("JoinCode") || string.IsNullOrEmpty(joinedLobby.Data["JoinCode"].Value)) && timeout > 0f)
        {
            await Task.Delay(500);
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            timeout -= 0.5f;
        }
        if (!joinedLobby.Data.ContainsKey("JoinCode")) return;

        var allocation = await RelayService.Instance.JoinAllocationAsync(joinedLobby.Data["JoinCode"].Value);
        var relayData = AllocationUtils.ToRelayServerData(allocation, RelayServerEndpoint.ConnectionTypeDtls);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
        NetworkManager.Singleton.StartClient();
    }

    private IEnumerator StartGameCountdown()
    {
        float countdown = gameCountdownTimer;
        while (countdown > 0)
        {
            UpdateCountdownValue(countdown);
            yield return new WaitForSeconds(1f);
            countdown -= 1f;
        }
        UpdateCountdownValue(0);
    }

    private async void UpdateCountdownValue(float value)
    {
        joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { KEY_COUNTDOWN_TIMER, new DataObject(DataObject.VisibilityOptions.Public, value.ToString()) }
            }
        });
        OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
    }

   
}
