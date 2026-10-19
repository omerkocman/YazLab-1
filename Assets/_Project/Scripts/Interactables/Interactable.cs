using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    
    [Header("Interaction Settings")]
    protected virtual string promptMessage => "Interact";

    private InteractableOutline _outline;

    protected virtual void Awake()
    {
        _outline = new InteractableOutline(gameObject);
    }

    public abstract void Interact();

    public virtual void Highlight()
    {
        if(_outline != null)
        {
            _outline.Enable();
        }
    }

    public virtual void Unhighlight()
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
}
