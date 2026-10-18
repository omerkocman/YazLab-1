using DG.Tweening;
using UnityEngine;

public class Toolbox : Openable
{
    [Header("Toolbox Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool revert = false;
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private Transform top;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

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

    protected override void OnOpen()
    {
        RotateLid(_openRotation);
    }

    protected override void OnClose()
    {
        RotateLid(_closedRotation);
    }

    private void RotateLid(Quaternion targetAngle)
    {
        top.DOKill();
        top.DOLocalRotateQuaternion(targetAngle, duration).SetEase(Ease.OutQuad);
    }
}