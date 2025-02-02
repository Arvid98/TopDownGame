using Unity.Collections;
using Unity.Netcode;
using System;

public struct InventoryItemData : INetworkSerializable, IEquatable<InventoryItemData>
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

    public override bool Equals(object obj)
    {
        return obj is InventoryItemData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return itemName.GetHashCode() ^ quantity.GetHashCode();
    }
}
