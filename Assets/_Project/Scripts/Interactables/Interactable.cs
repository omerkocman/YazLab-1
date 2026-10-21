using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    
    [Header("Interaction Settings")]
    protected virtual string promptMessage => "Interact";
    protected virtual string descriptionMessage => "Description";
    [SerializeField] private Transform _uiPoint;

    protected InteractableOutline _outline;

    protected virtual void Awake()
    {
        _outline = new InteractableOutline(gameObject);

    }

    public abstract void Interact(Inventory inventory);

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

    public string GetDescription()
    {
        return descriptionMessage;
    }

    public Vector3 GetUiPoint()
    {
        if(_uiPoint != null)
        {
            return _uiPoint.transform.position;
        }
        else
        {
            return default;
        }
    }
}
