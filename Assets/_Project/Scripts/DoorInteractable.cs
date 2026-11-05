using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(NavMeshObstacle))]
public class DoorInteractable : InteractableBase
{
    [Header("Door References")]
    [SerializeField] private bool isOpen;
    [SerializeField] private bool isLocked;
    [SerializeField, Range(0f, 2f)] private float duration = 1f;
    [SerializeField, Range(0f, 2f)] private float slideRange = 1.8f;
    [SerializeField] private Transform interactionPointA;
    [SerializeField] private Transform interactionPointB;
    [SerializeField] private Transform pivot;
    private bool isMoving;

    public bool IsOpen => isOpen;
    public bool IsMoving => isMoving;

    public Transform InteractionPositionA => interactionPointA != null ? interactionPointA : transform;
    public Transform InteractionPositionB => interactionPointB != null ? interactionPointB : transform;


    private NavMeshObstacle obstacle;


    private Vector3 openPosition;
    private Vector3 closePosition;

    protected override void Awake()
    {
        base.Awake();

        obstacle = GetComponent<NavMeshObstacle>();

        if (isLocked)
            isOpen = false;

        obstacle.enabled = !isOpen;

        if(pivot == null)
        {
            Debug.LogError("[DOOR INTERACTABLE] Pivot is null!", this);
            enabled = false;
            return;
        }

        closePosition = pivot.transform.localPosition;
        openPosition = pivot.transform.localPosition + Vector3.right * slideRange;

        if(isOpen)
        {
            pivot.localPosition = openPosition;
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
        if(!enabled || isLocked)
        {
            return false;
        }

        if(isMoving)
        {
            return false;
        }

        return true;

    }

    public bool UnlockAndOpen()
    {
        if (!isActiveAndEnabled || isMoving)
            return false;

        isLocked = false;
        if (!isOpen)
            Open();
        return true;
    }

    public override string GetUnavailableMessage(GameObject interactor)
    {
        return isLocked ? "Guard authorization is required." : base.GetUnavailableMessage(interactor);
    }

    public override string GetInteractionPrompt()
    {
        if(isMoving)
        {
            return "Door is moving...";
        }

        if (isLocked)
            return "Door locked";

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
        obstacle.enabled = true;
        Move(closePosition, false);

    }
    private void OnAnimationCompleted(bool targetOpen)
    {
        isOpen = targetOpen;
        isMoving = false;

        if(isOpen)
        {
            obstacle.enabled = false;
        }
    }

    private void Move(Vector3 targetPosition, bool targetOpen)
    {
        isMoving = true;

        pivot.DOLocalMove(targetPosition, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => OnAnimationCompleted(targetOpen));
    }

    private void OnDestroy()
    {
        pivot?.DOKill();
    }

   
}
