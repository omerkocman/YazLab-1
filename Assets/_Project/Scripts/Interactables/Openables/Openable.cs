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

        UpdatePropmtMessage();
    }

    public override void Interact()
    {
        if (isLocked)
        {
            return;
        }

        isOpen = !isOpen;

        UpdatePropmtMessage();

        if(isOpen)
        {
            OnOpen();
        }

        else
        {
            OnClose();
        }
     }

    private void UpdatePropmtMessage()
    {
        if (isLocked)
        {
            SetPropmtMessage(lockedPropmt);
        }

        else if (isOpen)
        {
            SetPropmtMessage(openPropmt);
        }
        else
        {
            SetPropmtMessage(closePropmt);
        }
    }
    protected abstract void OnOpen();
    protected abstract void OnClose();

}
