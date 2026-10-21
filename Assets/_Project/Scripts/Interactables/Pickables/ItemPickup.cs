using UnityEngine;

public class ItemPickup : Interactable
{
    [SerializeField] private ItemSO heldItem;
    [SerializeField] private int amount = 1;

    protected override string promptMessage => $"Pick up {heldItem.itemName}";
    protected override string descriptionMessage => heldItem.description;

    protected override void Awake()
    {
        base.Awake();
        
    }
    
    public override void Interact()
    {
        Inventory.Instance.AddItem(heldItem, amount);
        Destroy(gameObject);
    }
}

