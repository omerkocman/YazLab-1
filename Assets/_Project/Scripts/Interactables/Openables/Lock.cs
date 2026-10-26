using UnityEngine;

public class Lock : MonoBehaviour
{
    [SerializeField] private bool isLocked = true;
    [SerializeField] private ItemSO requiredKey;
    [SerializeField] private string lockedFeedback = "Requires keycard";

    public bool IsLocked => isLocked;
    public ItemSO RequiredKey => requiredKey;
    public string LockedFeedback => lockedFeedback;

    public bool CanUnlock(Inventory inventory)
    {
        if (!IsLocked) return true;

        if (requiredKey == null) return true;

        return inventory != null && inventory.HasItem(requiredKey);
    }

    public bool TryUnlock(Inventory inventory)
    {
        if (!isLocked) return true;

        if (CanUnlock(inventory))
        {
            if(requiredKey != null)
            {
                inventory.RemoveItem(requiredKey);
            }

            isLocked = false;
            return true;
        }

        return false;
    }

    public void ForceUnlock()
    {
        isLocked = false;
    }
}
