using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public class Chest : NetworkBehaviour
{

    private NetworkList<InventoryItemData> chestInventory;

    public NetworkVariable<ulong> currentUser = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        chestInventory = new NetworkList<InventoryItemData>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            chestInventory.OnListChanged += OnChestInventoryChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            chestInventory.OnListChanged -= OnChestInventoryChanged;
        }
    }

    private void OnChestInventoryChanged(NetworkListEvent<InventoryItemData> changeEvent)
    {
        Debug.Log("Chest inventory changed: " + changeEvent.Type);
    }

    public List<InventoryItemData> GetChestInventory()
    {
        List<InventoryItemData> list = new List<InventoryItemData>();
        for (int i = 0; i < chestInventory.Count; i++)
        {
            list.Add(chestInventory[i]);
        }
        return list;
    }
    public void AddItem(string itemName, int quantity)
    {
        if (!IsServer) return;

        int index = GetItemIndex(itemName);
        if (index >= 0)
        {
            var item = chestInventory[index];
            item.quantity += quantity;
            chestInventory[index] = item;
        }
        else
        {
            InventoryItemData newItem = new InventoryItemData
            {
                itemName = new FixedString32Bytes(itemName),
                quantity = quantity
            };
            chestInventory.Add(newItem);
        }
    }


    private int GetItemIndex(string itemName)
    {
        for (int i = 0; i < chestInventory.Count; i++)
        {
            if (chestInventory[i].itemName.ToString() == itemName)
            {
                return i;
            }
        }
        return -1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTransferItemFromPlayerServerRpc(string itemName, int quantity, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;


        if (currentUser.Value != clientId)
        {
            Debug.Log($"Chest is not open for client {clientId}");
            return;
        }

        NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (playerObject == null)
        {
            Debug.Log($"Player object not found for client {clientId}");
            return;
        }

        InventorySystem playerInventory = playerObject.GetComponent<InventorySystem>();
        if (playerInventory == null)
        {
            Debug.Log($"Player inventory not found for client {clientId}");
            return;
        }

        bool removed = playerInventory.TryRemoveItem(itemName, quantity);
        if (removed)
        {
            // Lägg till items i chestet
            AddItem(itemName, quantity);
            Debug.Log($"Transferred {quantity} {itemName} from player {clientId} to chest.");
        }
        else
        {
            Debug.Log($"Player {clientId} does not have enough {itemName} to transfer.");
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestOpenChestServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (currentUser.Value == 0)
        {
            currentUser.Value = clientId;
            Debug.Log($"Chest opened by client {clientId}");
        }
        else
        {
            Debug.Log($"Chest is already in use by client {currentUser.Value}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCloseChestServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (currentUser.Value == clientId)
        {
            currentUser.Value = 0;
            Debug.Log($"Chest closed by client {clientId}");
        }
        else
        {
            Debug.Log($"Client {clientId} attempted to close chest but is not the current user.");
        }
    }
}
