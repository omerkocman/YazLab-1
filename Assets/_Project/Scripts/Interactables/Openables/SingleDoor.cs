using DG.Tweening;
using UnityEngine;

public class SingleDoor : Openable
{
    [Header("Door Settings")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private bool revert = false;
    [SerializeField, Range(0, 2)] private float slideRange = 1.8f;
    private Vector3 _openPosition;
    private Vector3 _closedPosition;

    protected override void Awake()
    {
        base.Awake();

        _closedPosition = transform.localPosition;
        _openPosition = transform.localPosition + new Vector3(0f, 0f, revert ? -slideRange : slideRange);
        
        if (isOpen)
        {
            transform.localPosition = _openPosition;
        }
    }

    protected override void OnOpen()
    {
        MoveDoor(_openPosition);
    }

    protected override void OnClose()
    {
        MoveDoor(_closedPosition);        
    }

    private void MoveDoor(Vector3 targetPosition)
    {
        transform.DOKill();
        transform.DOLocalMove(targetPosition, duration).SetEase(Ease.OutQuad);
    }

}
