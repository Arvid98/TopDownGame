using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UI_PrefabSelector : MonoBehaviour
{
    [SerializeField] private NetworkPlayerSpawner spawner;
    [SerializeField] private Button[] prefabButtons;

    private void Start()
    {
        for (int i = 0; i < prefabButtons.Length; i++)
        {
            int index = i;
            prefabButtons[i].onClick.AddListener(() => SelectPrefab(index));
        }
    }

    private void SelectPrefab(int prefabIndex)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            spawner.SetPlayerPrefab(prefabIndex);
        }
    }
}
