using DG.Tweening;
using System;
using UnityEngine;

public class SingleDoor : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen;
    [SerializeField] private float duration = 1f;
    [SerializeField] private bool revert = false;
    [SerializeField, Range(0, 2)] private float slideRange = 1.8f;
    private Vector3 _openPosition;
    private Vector3 _closedPosition;

    protected override string promptMessage => isOpen ? "Close" : "Open";
    protected override string descriptionMessage => "";

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

    public override void Interact()
    {
        if(isOpen)
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

}
