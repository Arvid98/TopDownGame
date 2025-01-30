using Unity.Collections;
using Unity.Netcode;

public struct InventoryItemData : INetworkSerializable, System.IEquatable<InventoryItemData>
{
    public FixedString32Bytes itemName; 
    public int quantity; 

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref quantity);
    }

    public bool Equals(InventoryItemData other)
    {
        return itemName.Equals(other.itemName) && quantity == other.quantity;
    }

    public override int GetHashCode()
    {
        return itemName.GetHashCode() ^ quantity.GetHashCode();
    }
}
