using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public class Host : MonoBehaviour
{
    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private bool isInitialized = false;


    public GameObject playerPrefab;

    private PlayerSpawner playerSpawner;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started");

        playerSpawner = new PlayerSpawner(playerPrefab);

        SpawnHostPlayer();
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Vector3 spawnPosition = GetSpawnPositionForClient(clientId);
            playerSpawner.SpawnPlayer(clientId, spawnPosition);
        };
    }

    private void SpawnHostPlayer()
    {
        Vector3 spawnPosition = GetSpawnPositionForClient(NetworkManager.Singleton.LocalClientId);
        playerSpawner.SpawnPlayer(NetworkManager.Singleton.LocalClientId, spawnPosition);
    }
    private Vector3 GetSpawnPositionForClient(ulong clientId)
    {
        
        return new Vector3(clientId * 2.0f, 0f, 0f); 
    }
    void Update()
    {
        if (!isInitialized) return;

        driver.ScheduleUpdate().Complete();
        AcceptConnections();
        //HandleMessages();
    }

    private void AcceptConnections()
    {
        NetworkConnection connection;
        while ((connection = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(connection);
            Debug.Log("New connection accepted");
        }

        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }

    //private void HandleMessages()
    //{
    //    DataStreamReader stream;
    //    for (int i = 0; i < connections.Length; i++)
    //    {
    //        NetworkEvent.Type cmd;
    //        while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
    //        {
    //            if (cmd == NetworkEvent.Type.Data)
    //            {
    //                string message = stream.ReadFixedString32().ToString();
    //                Debug.Log($"Received: {message}");
    //                SendMessage(connections[i], $"Echo: {message}");
    //            }
    //            else if (cmd == NetworkEvent.Type.Disconnect)
    //            {
    //                Debug.Log("Client disconnected");
    //                connections[i] = default(NetworkConnection);
    //            }
    //        }
    //    }
    //}

    private void SendMessage(NetworkConnection connection, string message)
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
        if (driver.IsCreated) driver.Dispose();
        if (connections.IsCreated) connections.Dispose();
    }
}
