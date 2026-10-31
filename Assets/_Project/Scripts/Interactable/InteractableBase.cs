using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    protected InteractableOutline Highlight { get; private set; }

    [Header("Outline Colors")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color unavailableColor = Color.red;


    protected virtual void Awake()
    {
        Highlight = new InteractableOutline(gameObject);

    }

    public abstract void Interact(GameObject interactor);

    public virtual bool CanInteract(GameObject interactor)
    {
        return true;
    }

    public virtual string GetInteractionPrompt()
    {
        return "E - Interact";
    }

    public virtual string GetUnavailableMessage(GameObject interactor)
    {
        return "This object is currently unavailable.";
    }

    public void SetHighlighted(bool active, GameObject interactor)
    {
        if(!active)
        {
            Highlight.Disable();
            return;
        }

        Color color = CanInteract(interactor) ? availableColor : unavailableColor;

        Highlight.ChangeColor(color);
        Highlight.Enable();

    }

    public void RefreshHighlight(GameObject interactor)
    {
        SetHighlighted(true, interactor);
    }

    private void OnDisable()
    {
        Highlight?.Disable();
    }

}
