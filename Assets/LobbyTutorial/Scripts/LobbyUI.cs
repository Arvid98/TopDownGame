using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour {


    public static LobbyUI Instance { get; private set; }


    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button changeCaveManButton;
    [SerializeField] private Button changeCaveWomanButton;
    [SerializeField] private Button changeCaveManTwoButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button changeGameModeButton;
    [SerializeField] private Button startGameButton;


    private void Awake() {
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);

        changeCaveManButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Cock);
        });
        changeCaveWomanButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Builder);
        });
        changeCaveManTwoButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.BlackSmith);
        });

        leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });

        changeGameModeButton.onClick.AddListener(() => {
            LobbyManager.Instance.ChangeGameMode();
        });

        startGameButton.onClick.AddListener(() => {
            LobbyManager.Instance.StartGame();
        });
    }

    private void Start() {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameModeChanged += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;

        Hide();
    }


    //private void Update()
    //{
    //    if (!LobbyManager.Instance.IsLobbyHost()) return;

    //    // Temporayr debug
    //    int totalClients = NetworkManager.Singleton.ConnectedClientsList.Count;
    //    startGameButton.interactable = totalClients > 1;
    //}

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e) {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        UpdateLobby();
       
    }
    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        
        if (!LobbyManager.Instance.IsLobbyHost())
            LobbyManager.Instance.JoinRelayAndStartClient();

        UpdateLobby();  
    }


    private void UpdateLobby() {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby) {
        ClearLobby();

        foreach (Player player in lobby.Players) {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        changeGameModeButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameModeText.text = lobby.Data[LobbyManager.KEY_GAME_MODE].Value;
        if (lobby.Data.ContainsKey(LobbyManager.KEY_COUNTDOWN_TIMER))
        {
          
            if (float.TryParse(lobby.Data[LobbyManager.KEY_COUNTDOWN_TIMER].Value, out float timerValue))
            {
                timerText.text = Mathf.Ceil(timerValue).ToString();
            }
            else
            {
                timerText.text = lobby.Data[LobbyManager.KEY_COUNTDOWN_TIMER].Value;
            }
        }
        else
        {
            timerText.text = "Waiting...";
        }

        Show();
    }

    private void ClearLobby() {
        if(container != null)
        {
            foreach (Transform child in container)
            {
                if (child == playerSingleTemplate) continue;
                Destroy(child.gameObject);
            }
        }
        
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

}