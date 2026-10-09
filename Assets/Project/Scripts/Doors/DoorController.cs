using UnityEngine;

[SelectionBase]
public class DoorController : MonoBehaviour
{
    [Header("Angle Settings")]
    [Range(0, 180)]
    public float openAngle = 90f;
    public bool reverse = false;

    [Header("Door Mode")]
    public bool isRotatingDoor = true;
    public bool isOpen = false;



    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;
    private bool lastIsRotatingDoor;

    void Start()
    {
        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;

        lastIsRotatingDoor = isRotatingDoor;
    }

    void Update()
    {
        if(isRotatingDoor != lastIsRotatingDoor)
        {
            ResetDoorState();
            lastIsRotatingDoor = isRotatingDoor;
        }
        if (isRotatingDoor)
        {
            AnimateRotatingDoor();

        }
        else
        {
            AnimateSlidingDoor();
        }
    }

    private void AnimateRotatingDoor()
    {
        Quaternion openRotation = initialLocalRotation * Quaternion.Euler(0f, openAngle * (reverse ? -1 : 1), 0f);
        Quaternion targetRotation = isOpen ? openRotation : initialLocalRotation;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 5f);
    }

    private void AnimateSlidingDoor()
    {
        Vector3 openPosition = initialLocalPosition + new Vector3(0f, 0f, -(2f - 0.1f) * (reverse ? -1 : 1));
        
        Vector3 targetPosition = isOpen ? openPosition : initialLocalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 5f);
    }

    private void ResetDoorState()
    {
        transform.localRotation = initialLocalRotation;
        transform.localPosition = initialLocalPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            isOpen = true;
            print("Player trigger alanına girdi!");
        }
    }
}
