using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<InventorySlot> inventory = new List<InventorySlot>();

    public IReadOnlyList<InventorySlot> GetInventoryList() => inventory;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        //if(Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //Instance = this;
    }

    public void AddItem(ItemSO item, int amount = 1)
    {
        if (amount <= 0) return;

        InventorySlot slot = GetSlotByItem(item);

        if(slot != null)
        {
            slot.AddAmount(amount);
        }
        else
        {
            inventory.Add(new InventorySlot(item, amount));
        }

        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(ItemSO item, int amount = 1)
    {
        InventorySlot slot = GetSlotByItem(item);

        if(slot == null ||slot.itemAmount < amount)
        {
            return false;
        }

        if(slot.RemoveAmount(amount))
        {
            inventory.Remove(slot);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    private InventorySlot GetSlotByItem(ItemSO item)
    {
        if(item == null)
        {
            return null;
        }

        foreach(InventorySlot slot in inventory)
        {
            if(slot.heldItem == item)
            {
                return slot;
            }
        }

        return null;
    }

    public bool HasItem(ItemSO item, int requiredAmount = 1)
    {
        InventorySlot slot = GetSlotByItem(item);

        if(slot != null)
        {
            if(slot.itemAmount >= requiredAmount)
            {
                return true;
            }
        }

        return false;
    }
}
