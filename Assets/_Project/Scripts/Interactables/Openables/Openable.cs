using UnityEngine;

public abstract class Openable : Interactable
{
    [Header("Openable Settings")]
    [SerializeField] protected bool isOpen = false;
    [SerializeField] protected bool isLocked = false;

    private string openPropmt = "Close";
    private string closePropmt = "Open";
    private string lockedPropmt = "Locked";

    private string messagePropmt;

    protected override void Awake()
    {
        base.Awake();

    }

    public override void Interact()
    {
        if (isLocked)
        {
            return;
        }

        isOpen = !isOpen;

        if(isOpen)
        {
            OnOpen();
        }

        else
        {
            OnClose();
        }
     }

    protected abstract void OnOpen();
    protected abstract void OnClose();

}
