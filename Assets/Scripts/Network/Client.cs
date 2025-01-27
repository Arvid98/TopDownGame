using Unity.Collections;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    private NetworkDriver driver;
    private NetworkConnection connection;
    private bool isInitialized = false;

    public GameObject playerPrefab;


    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client connecting...");

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log($"Client {clientId} connected to the server.");
            }
        };
    }

    void Update()
    {
        if (isInitialized)
        {
            {
                driver.ScheduleUpdate().Complete();

                if (!connection.IsCreated)
                {
                    Debug.LogWarning("Failed to connect to server");
                    return;
                }

                //HandleMessages();
            }
        }
    }

    //private void HandleMessages()
    //{
    //    DataStreamReader stream;
    //    NetworkEvent.Type cmd;
    //    while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
    //    {
    //        if (cmd == NetworkEvent.Type.Connect)
    //        {
    //            Debug.Log("Connected to server!");
    //            SendMessage("Hello, server!");
    //        }
    //        else if (cmd == NetworkEvent.Type.Data)
    //        {
    //            string message = stream.ReadFixedString32().ToString();
    //            Debug.Log($"Received from server: {message}");
    //        }
    //        else if (cmd == NetworkEvent.Type.Disconnect)
    //        {
    //            Debug.Log("Disconnected from server");
    //            connection = default(NetworkConnection);
    //        }
    //    }
    //}

    private void SendMessage(string message)
    {
        DataStreamWriter writer;
        int result = driver.BeginSend(connection, out writer);
        if (result == 0)
        {
            writer.WriteFixedString32(message);
            driver.EndSend(writer);
        }
        else
        {
            Debug.LogError("Failed to send message");
        }
    }

    void OnDestroy()
    {
        if (driver.IsCreated)
        {
            driver.Dispose();
            Debug.Log("NetworkDriver disposed.");
        }
    }

}
