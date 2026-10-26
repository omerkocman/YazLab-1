using UnityEngine;

public enum InteractableType
{
    None,
    Collectable,
    Door,
    Switch
}
public abstract class Interactable : MonoBehaviour
{
    
    [Header("Interaction Settings")]
    protected virtual string promptMessage => "Interact";
    protected virtual string descriptionMessage => "Description";
    public InteractableType type = InteractableType.None;
    [SerializeField] private Transform interactionPoint;
    public Vector3 TargetPoint => interactionPoint != null ? interactionPoint.position : transform.position;

    protected InteractableOutline _outline;

    protected virtual void Awake()
    {
        _outline = new InteractableOutline(gameObject);

    }

    public abstract void Interact(Player player);

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
        if(interactionPoint != null)
        {
            return interactionPoint.transform.position;
        }
        else
        {
            return default;
        }
    }
}
