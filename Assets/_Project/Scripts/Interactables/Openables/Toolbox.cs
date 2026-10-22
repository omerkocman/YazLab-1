using DG.Tweening;
using UnityEngine;

public class Toolbox : Interactable
{
    [Header("Toolbox Settings")]
    [SerializeField] private bool isOpen;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private Transform top;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    protected override string promptMessage => isOpen ? $"Close {nameof(Toolbox)}" : "Open";
    protected override string descriptionMessage => "";

    protected override void Awake()
    {
        base.Awake();

        _closedRotation = top.transform.localRotation;
        _openRotation = _closedRotation * Quaternion.Euler(openAngle, 0f, 0f);

        if(isOpen)
        {
            top.transform.localRotation = _openRotation;
        }

    }

    public override void Interact(Player player)
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
        RotateLid(_openRotation);
        isOpen = true;
    }

    protected void OnClose()
    {
        RotateLid(_closedRotation);
        isOpen = false;
    }

   
    private void RotateLid(Quaternion targetAngle)
    {
        top.DOKill();
        top.DOLocalRotateQuaternion(targetAngle, duration).SetEase(Ease.OutQuad);
    }
}