using UnityEngine;

public interface IInventory
{
    void AddItem(ItemBase item);

    bool RemoveItem(ItemBase item);

    bool HasKey(string keyId);
}
