using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum ItemType
{
    Weapon,
    Currency
}

[Serializable]
public class InventorySlot
{
    [SerializeField] private ItemSO heldItem;
    [SerializeField] private int itemAmount;

    //private Image icon;
    //private TextMeshProUGUI amountText;

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

        //UpdateSlot();
    }

    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        //UpdateSlot();
        return itemAmount;
    }

    public int RemoveAmount(int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if (itemAmount <= 0)
        {
            ClearSlot();
        }

        else
        {
            //UpdateSlot();
        }

        return itemAmount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;

        //UpdateSlot();
    }

    public bool HasAnyItem()
    {
        return heldItem != null;
    }

    //private void UpdateSlot()
    //{
    //    if(heldItem != null)
    //    {
    //        icon.enabled = true;
    //        icon.sprite = heldItem.icon;
    //        amountText.text = itemAmount.ToString();
    //    }

    //    else
    //    {
    //        icon.enabled = false;
    //        amountText.text = "";
    //    }
    //}
}
