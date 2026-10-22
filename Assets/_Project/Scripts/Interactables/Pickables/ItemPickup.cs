using UnityEngine;

public class ItemPickup : Interactable
{
    [SerializeField] private ItemSO heldItem;
    [SerializeField] private int amount = 1;

    protected override string promptMessage => $"Pick up <color=#{heldItem.itemType.GetHexColor()}>{heldItem.itemName}</color>";
    protected override string descriptionMessage => heldItem.description;

    protected override void Awake()
    {
        base.Awake();
        
    }
    
    public override void Interact(Player player)
    {
        player.Inventory.AddItem(heldItem, amount);
        Destroy(gameObject);
    }
}

