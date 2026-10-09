using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, IInventory
{
    [SerializeField]
    public List<ItemBase> _items = new List<ItemBase>();
    
    public void AddItem(ItemBase item)
    {
        if(item == null)
        {
            return;
        }

        _items.Add(item);
        Debug.Log($"[Inventory] {item.itemName} added to inventory");
    }

    public bool RemoveItem(ItemBase item)
    {
        return _items.Remove(item);
    }

    public bool HasKey(string requiredKeyId)
    {
        if(String.IsNullOrEmpty(requiredKeyId))
        {
            return true;
        }

        foreach(ItemBase item in _items)
        {
            if(item is KeyItem key && key.keyId == requiredKeyId)
            {
                return true;
            }
        }

        return false;
    }
}
