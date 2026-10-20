using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    private static int inventorySize = 32;
    [SerializeField] public List<InventorySlot> inventory = new List<InventorySlot>();
    public IReadOnlyList<InventorySlot> Slots => inventory;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeInventory();
    }

    private void InitializeInventory()
    {
        inventory.Clear();

        for(int i = 0; i < inventorySize; i++)
        {
            inventory.Add(new InventorySlot());
        }
        Debug.Log($"Inventory slot count: {inventory.Count}");
    }

    public void AddItem(ItemSO item, int amount = 1)
    {
        if(item == null || amount <= 0)
        {
            return;
        }

        int emptySlot = GetFirstEmptySlot();

        if(emptySlot == -1)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        inventory[emptySlot].SetItem(item, amount);
    }

    public bool HasItem(ItemSO item)
    {
        foreach(InventorySlot slot in inventory)
        {
            if(slot.GetItem() == item)
            {
                return true;
            }
        }

        return false;
    }

    public int GetFirstEmptySlot()
    {
        for(int i = 0; i< inventory.Count; i++)
        {
            if (!inventory[i].HasAnyItem())
            {
                return i;
            }
        }

        return -1;
    }

    public int GetEmptySlotCount()
    {
        int count = 0;
        foreach(InventorySlot slot in inventory)
        {
            if(!slot.HasAnyItem())
            {
                count++;
            }
        }

        return count;
    }
}
