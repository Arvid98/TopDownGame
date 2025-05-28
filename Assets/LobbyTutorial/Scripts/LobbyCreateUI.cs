using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{

    public static LobbyCreateUI Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button publicPrivateButton;
    [SerializeField] private Button gameModeButton;

    [Header("Display Text")]
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI publicPrivateText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;
    [SerializeField] private TextMeshProUGUI gameModeText;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_InputField maxPlayersInputField;

    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;
    private LobbyManager.GameMode gameMode;

    private void Awake()
    {
        Instance = this;


        createButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(
                lobbyName,
                maxPlayers,
                isPrivate,
                gameMode
            );
            Hide();
        });


        lobbyNameInputField.onEndEdit.AddListener(HandleLobbyNameEdit);
        maxPlayersInputField.onEndEdit.AddListener(HandleMaxPlayersEdit);

        publicPrivateButton.onClick.AddListener(() => {
            isPrivate = !isPrivate;
            UpdateText();
        });


        gameModeButton.onClick.AddListener(() => {
            switch (gameMode)
            {
                default:
                case LobbyManager.GameMode.Creative:
                    gameMode = LobbyManager.GameMode.Survivel;
                    break;
                case LobbyManager.GameMode.Survivel:
                    gameMode = LobbyManager.GameMode.Creative;
                    break;
            }
            UpdateText();
        });

        Hide();
    }


    private void HandleLobbyNameEdit(string newName)
    {
        lobbyName = newName;
        UpdateText();
    }


    private void HandleMaxPlayersEdit(string newMaxPlayers)
    {
        if (int.TryParse(newMaxPlayers, out int value))
        {
            maxPlayers = value;
        }
        else
        {
           
            maxPlayers = 4;
        }
        UpdateText();
    }

    private void UpdateText()
    {
        lobbyNameText.text = lobbyName;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        maxPlayersText.text = maxPlayers.ToString();
        gameModeText.text = gameMode.ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);


        lobbyName = "MyLobby";
        isPrivate = false;
        maxPlayers = 4;
        gameMode = LobbyManager.GameMode.Creative;

        if (lobbyNameInputField != null)
        {
            lobbyNameInputField.text = lobbyName;
        }
        if (maxPlayersInputField != null)
        {
            maxPlayersInputField.text = maxPlayers.ToString();
        }

        UpdateText();
    }
}
