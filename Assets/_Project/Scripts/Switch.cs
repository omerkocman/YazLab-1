using DG.Tweening;
using UnityEngine;

[SelectionBase]
public class Switch : InteractableBase
{
    [Header("Gas Control")]
    [SerializeField] private PoisonGasVent gasVent;

    [Header("Switch References")]
    [SerializeField] private bool isOpen;
    [SerializeField, Range(0f, 2f)] private float duration = 1f;
    [SerializeField, Range(0f, 180f)] private float rotateRange = 90f;
    [SerializeField] private Transform pivot;

    [SerializeField] private Transform interactionPoint;

    private Vector3 openPosition;
    private Vector3 closePosition;
    private bool isMoving;


    public Vector3 InteractionPosition => interactionPoint != null ? interactionPoint.position : transform.position;

    public bool IsOpen => isOpen;
    public bool IsMoving => isMoving;


    private void Start()
    {
        if(gasVent != null)
        {
            gasVent.SetGasActive(isOpen);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        closePosition = pivot.localEulerAngles;
        openPosition = pivot.localEulerAngles + new Vector3(0f, 0f, rotateRange);

        if(isOpen)
        {
            pivot.localEulerAngles = openPosition;
        }
    }

    public override void Interact(GameObject interactor)
    {
        if (CanInteract(interactor))
        {
            Toggle();
        }
    }

    public override bool CanInteract(GameObject interactor)
    {
        return enabled && !isMoving;
    }

    public override string GetInteractionPrompt()
    {
        return isOpen ? "Close Switch" : "Open Switch";
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
        Rotate(openPosition, true);

    }

    private void Close()
    {
        Rotate(closePosition, false);
    }

    private void OnAnimationCompleted(bool targetOpen)
    {
        isOpen = targetOpen;
        isMoving = false;

        if(gasVent != null)
        {
            gasVent.SetGasActive(isOpen);
        }
    }

    private void Rotate(Vector3 targetRotation, bool targetOpen)
    {
        isMoving = true;

        pivot.DOLocalRotate(targetRotation, duration)
            .OnComplete(() => OnAnimationCompleted(targetOpen));
    }
}
