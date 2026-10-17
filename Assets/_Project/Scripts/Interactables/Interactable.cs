using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    
    [Header("Interaction Settings")]
    [SerializeField] protected string promptMessage = "Interact";

    private InteractableOutline _outline;

    protected virtual void Awake()
    {
        _outline = GetComponent<InteractableOutline>();
    }

    public abstract void Interact();

    public virtual void OnFocus()
    {
        if(_outline != null)
        {
            _outline.Enable();
        }
    }

    public virtual void OnLoseFocus()
    {
        if(_outline != null)
        {
            _outline.Disable();
        }
    }

    public string GetPropmt()
    {
        return promptMessage;
    }

    protected void SetPropmtMessage(string newPropmtMessage)
    {
        promptMessage = newPropmtMessage;
    }
}
