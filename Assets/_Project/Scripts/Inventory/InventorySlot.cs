using System;

[Serializable]
public class InventorySlot
{
    public ItemSO heldItem;
    public int itemAmount;

    public InventorySlot(ItemSO heldItem, int itemAmount)
    {
        this.heldItem = heldItem;
        this.itemAmount = itemAmount;
    }

    public void AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
    }

    public bool RemoveAmount(int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if(itemAmount <= 0)
        {
            ClearSlot();

            return true;
        }

        return false;
    }

    private void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;
    }
}
