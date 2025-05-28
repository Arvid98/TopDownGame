using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerName : MonoBehaviour
{
    public static EditPlayerName Instance { get; private set; }
    public event EventHandler OnNameChanged;

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TMP_InputField playerNameInputField;

    private string playerName = "Enter Name";

    private void Awake()
    {
        Instance = this;

        playerNameText.text = playerName;
        playerNameInputField.text = playerName;

        playerNameInputField.onEndEdit.AddListener(HandleInputEndEdit);
    }

    private void HandleInputEndEdit(string newName)
    {
        playerName = newName;
        playerNameText.text = playerName;
        OnNameChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetPlayerName()
    {
        return playerName;
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
