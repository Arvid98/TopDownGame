using UnityEngine;
using Unity.Netcode;

public class ChestTransferTest : MonoBehaviour
{

    [SerializeField] private Chest chest;

    private void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.T))
        {
      
            if (chest != null)
            {
                chest.RequestTransferItemFromPlayerServerRpc("Sword", 1);
            }
        }
    }
}
