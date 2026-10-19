using System;
using TMPro;
using UnityEngine;

enum ItemType
{
    Weapon,
    Currency
}

[Serializable]
public class Slot
{
    [SerializeField] private ItemSO heldItem;
    [SerializeField] private int itemAmount;

    private Sprite icon;
    private TextMeshProUGUI amountText;

    public ItemSO GetItem()
    {
        return heldItem;
    }

    public int GetAmount()
    {
        return itemAmount;
    }

    public void SetItem(ItemSO newItem, int amount = 1)
    {
        heldItem = newItem;
        itemAmount = amount;

        // UpdateSlot()
    }

    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        // UpdateSlot()
        return itemAmount;
    }

    public int RemoveAmount(int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if(itemAmount <= 0)
        {
            ClearSlot();
        }

        else
        {
            // UpdateSlot()
        }

        return itemAmount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;

        // UpdateSlot
    }

    public bool HasAnyItem()
    {
        return heldItem != null;
    }
}
