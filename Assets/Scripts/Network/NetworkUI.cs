using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    public GameObject hostObject;
    public GameObject clientObject;
    public void StartHost()
    {
        //hostObject.SetActive(true);
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        //clientObject.SetActive(true);
        
        NetworkManager.Singleton.StartClient();
    }
}
