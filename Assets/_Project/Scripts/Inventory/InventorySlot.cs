using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount;

    public bool IsEmpty()
    {
        bool isEmpty = (itemData == null || amount == 0);

        return isEmpty;
    }

    public bool IsFull()
    {
        int maxStackSize = itemData.maxStackSize;
        bool isFull = amount >= maxStackSize;

        return isFull;
    }

    public int FreeSpace()
    {
        int maxStackSize = itemData.maxStackSize;
        int freeSpace = maxStackSize - amount;

        return freeSpace;
    }

    public void SetItem(ItemData newItemData)
    {
        itemData = newItemData;
    }

    public void SetAmount(int newAmount)
    {
        amount = newAmount;
    }

    public bool CanStack(ItemData item)
    {
        if(itemData == item)
        {
            return IsEmpty();
        }
        if(itemData == null)
        {
            return true;
        }

        return false;
    }

    public void Clear()
    {
        itemData = null;
        amount = 0;
    }
}
