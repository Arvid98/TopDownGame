using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public Client clientScript;
    public Host hostScript;
    public Button[] hostPrefabButtons;
    public Button[] clientPrefabButtons;
    public GameObject hostButton;
    public GameObject clientButton;
    public GameObject uiContainer;

    private bool hostPrefabSelected = false;
    private bool clientPrefabSelected = false;

    void Start()
    {
        hostButton.SetActive(false);
        clientButton.SetActive(false);

        for (int i = 0; i < hostPrefabButtons.Length; i++)
        {
            int index = i;
            hostPrefabButtons[i].onClick.AddListener(() =>
            {
                hostScript.SelectPrefab(index);
                hostPrefabSelected = true;
                UpdateButtonStates();
            });
        }

        for (int i = 0; i < clientPrefabButtons.Length; i++)
        {
            int index = i;
            clientPrefabButtons[i].onClick.AddListener(() =>
            {
                clientScript.SelectPrefab(index);
                clientPrefabSelected = true;
                UpdateButtonStates();
            });
        }

        hostButton.GetComponent<Button>().onClick.AddListener(() => StartGameAsHost());
        clientButton.GetComponent<Button>().onClick.AddListener(() => StartGameAsClient());
    }

    private void UpdateButtonStates()
    {
        hostButton.SetActive(hostPrefabSelected);
        clientButton.SetActive(clientPrefabSelected);
    }

    private void StartGameAsHost()
    {
        hostScript.StartHost();
        HideAllUI();
    }

    private void StartGameAsClient()
    {
        clientScript.StartClient();
        HideAllUI();
    }

    private void HideAllUI()
    {
        uiContainer.SetActive(false);
    }
}
