using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    private static int inventorySize = 10;
    [SerializeField] private List<Slot> inventory = new List<Slot>();

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
    }

    public void AddItem(ItemSO item, int amount = 1)
    {
        Slot newSlot = new Slot();
        newSlot.SetItem(item, amount);
        inventory.Add(newSlot);
    }

    public bool HasItem(ItemSO item)
    {
        foreach(Slot slot in inventory)
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
            bool hasItem = inventory[i].HasAnyItem();
            if(!hasItem)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetEmptySlotCount()
    {
        int count = 0;
        foreach(Slot slot in inventory)
        {
            if(!slot.HasAnyItem())
            {
                count++;
            }
        }

        return count;
    }
}
