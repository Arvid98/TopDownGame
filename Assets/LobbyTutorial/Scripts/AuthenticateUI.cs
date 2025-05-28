using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour
{
    [SerializeField] private Button authenticateButton;
    [SerializeField] private TMPro.TextMeshProUGUI errorText;

    private void Awake()
    {
        authenticateButton.onClick.AddListener(OnAuthenticateClicked);
    
        EditPlayerName.Instance.OnNameChanged += HandleNameChanged;
    }

    private void OnDestroy()
    {
   
        if (EditPlayerName.Instance != null)
            EditPlayerName.Instance.OnNameChanged -= HandleNameChanged;
    }

    private void HandleNameChanged(object sender, EventArgs e)
    {
        string playerName = EditPlayerName.Instance.GetPlayerName();
   
        bool isValid = !string.IsNullOrWhiteSpace(playerName) && playerName != "Enter Name";
        authenticateButton.interactable = isValid;

       
        if (errorText != null)
        {
            errorText.text = isValid ? "" : "Enter Name!";
        }
    }

    private void OnAuthenticateClicked()
    {
  
        LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());
        Hide();
        EditPlayerName.Instance.Hide();
       
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
