using DG.Tweening;
using UnityEngine;

[SelectionBase]
public class DoorInteractable : InteractableBase
{
    [Header("Door References")]
    [SerializeField] private bool isOpen;
    [SerializeField, Range(0f, 2f)] private float duration = 1f;
    [SerializeField, Range(0f, 2f)] private float slideRange = 1.8f;
    [SerializeField] private Transform hingePivot;

    private Vector3 openPosition;
    private Vector3 closePosition;

    private bool isMoving;

    protected override void Awake()
    {
        base.Awake();

        if(hingePivot == null)
        {
            Debug.LogError("[DOOR INTERACTABLE] HingePivot is null!", this);
            enabled = false;
            return;
        }

        closePosition = hingePivot.transform.localPosition;
        openPosition = hingePivot.transform.localPosition + Vector3.right * slideRange;

        if(isOpen)
        {
            hingePivot.localPosition = openPosition;
        }

    }

    public override void Interact(GameObject interactor)  
    {
        if(CanInteract(interactor))
        {
            Toggle();
        }
    }

    public override bool CanInteract(GameObject interactor)
    {
        if(!enabled)
        {
            return false;
        }

        if(isMoving)
        {
            return false;
        }

        return true;

    }

    public override string GetInteractionPrompt()
    {
        if(isMoving)
        {
            return "Door is moving...";
        }

        return isOpen ? "Close Door" : "Open Door";
    }

    private void Toggle()
    {

        if(isOpen)
        {
            Close();
        }

        else
        {
            Open();
        }
    }

    private void Open()
    {
        Move(openPosition, true);
    }

    private void Close()
    {
        Move(closePosition, false);

    }
    private void OnAnimationCompleted(bool targetOpen)
    {
        isOpen = targetOpen;
        isMoving = false;
    }

    private void Move(Vector3 targetPosition, bool targetOpen)
    {
        isMoving = true;

        hingePivot.DOLocalMove(targetPosition, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => OnAnimationCompleted(targetOpen));
    }

    private void OnDestroy()
    {
        hingePivot?.DOKill();
    }

   
}
