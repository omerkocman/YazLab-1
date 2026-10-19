using UnityEngine;

public class ItemPickup : Interactable
{
    [SerializeField] private ItemSO itemData;
    [SerializeField] private int amount = 1;

    protected override string promptMessage => $"Pickup ";

    protected override void Awake()
    {
        base.Awake();
    }
    
    public override void Interact()
    {
        Inventory.Instance.AddItem(itemData, amount);
        Destroy(gameObject);
    }
}

