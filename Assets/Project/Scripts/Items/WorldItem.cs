using System;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("Item Data")]
    [Tooltip("Upload .asset file")]
    public ItemBase itemData;
    public void Interact(IInventory inventory)
    {
       // PickUp(IInventory targetInventory);
    }

    public ItemBase PickUp(IInventory targetInventory)
    {
        Debug.Log($"{itemData.itemName} picked up!");
        if(targetInventory != null && itemData != null)
        {
            targetInventory.AddItem(itemData);
        }
        Destroy(gameObject);

        return itemData;
    }
}
