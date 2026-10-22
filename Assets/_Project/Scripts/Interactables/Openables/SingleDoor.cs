using DG.Tweening;
using System;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;

public class SingleDoor : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private string doorName = "Security Door";
    [SerializeField] private bool isOpen;
    [SerializeField] private float duration = 1f;
    [SerializeField] private bool revert = false;
    [SerializeField, Range(0, 2)] private float slideRange = 1.8f;

    [SerializeField] private Lock doorLock;
    private Vector3 _openPosition;
    private Vector3 _closedPosition;
    protected override string descriptionMessage => (doorLock != null && doorLock.IsLocked) ? $"Requires {doorLock.RequiredKey.itemName}" : "";

    protected override void Awake()
    {
        base.Awake();

        slideRange = -slideRange;

        _closedPosition = transform.localPosition;
        _openPosition = transform.localPosition + new Vector3(0f, 0f, revert ? -slideRange : slideRange);
        
        if (isOpen)
        {
            transform.localPosition = _openPosition;
        }
    }

    public override void Interact(Player player)
    {
        if(doorLock != null && doorLock.IsLocked)
        {
            bool unlocked = doorLock.TryUnlock(player.Inventory);

            if(!unlocked)
            {
                Debug.Log(doorLock.LockedFeedback);
                return;
            }

        }

        ToggleDoor();
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            OnClose();
        }
        else
        {
            OnOpen();
        }
    }

    protected void OnOpen()
    {
        MoveDoor(_openPosition);
        isOpen = true;
    }

    protected void OnClose()
    {
        MoveDoor(_closedPosition);
        isOpen = false;
    }

    

    private void MoveDoor(Vector3 targetPosition)
    {
        transform.DOKill();
        transform.DOLocalMove(targetPosition, duration).SetEase(Ease.OutQuad);
    }

    protected override string promptMessage
    {
        get
        {
            if (doorLock != null && doorLock.IsLocked)
            {
                _outline.ChangeColor(Color.red);
                return $"Unlock <color=#E63946>{doorName}</color>";
            }

            _outline.ChangeColor(Color.white);

            return isOpen
                ? $"Close <color=#7D8DA1>{doorName}</color>"
                : $"Open <color=#52B788>{doorName}</color>";
        }
    }
}
