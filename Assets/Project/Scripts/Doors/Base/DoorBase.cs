using UnityEngine;

public class DoorBase : MonoBehaviour, IInteractable
{
    [Header("Basic Settings")]
    public bool isOpen = false;
    public float transitionSpeed = 5f;
    public Transform doorMesh;

    [Header("Lock Settings")]
    public bool isLocked = false;
    [Tooltip("if its")]
    public string requiredKeyId = "";

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(IInventory inventory)
    {
        if(isLocked)
        {
            if(inventory.HasKey(requiredKeyId))
            {
                isLocked = false;
            }
            else
            {
                Debug.Log($"{gameObject.name} is locked! Required key: {requiredKeyId}");
                return;
            }
        }

        isOpen = !isOpen;
    }

    //public void TryUnlock(string keyId)
    //{

    //}
}
