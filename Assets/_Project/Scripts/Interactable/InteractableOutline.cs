using UnityEngine;

public class InteractableOutline
{
    private readonly Outline outline;
    
    public InteractableOutline(GameObject gameObject)
    {
        outline = gameObject.GetComponentInChildren<Outline>(true);

        if(outline == null)
        {
            Debug.Log($"[INTERACTABLE OUTLINE] {gameObject.name} has no Outline.");
            return;
        }

        ChangeSettings(Outline.Mode.OutlineVisible, 10f, Color.white);

        Disable();

    }

    public void Enable()
    {
        if (outline == null)
        {
            return;
        }
        
        outline.enabled = true;
    
    }

    public void Disable()
    {
        if (outline == null)
        {
            return;
        }

        outline.enabled = false;

    }

    public void ChangeMode(Outline.Mode newMode)
    {
        if(outline == null)
        {
            return;
        }

        outline.OutlineMode = newMode;
    }
    public void ChangeWidth(float newWidth)
    {
        if (outline == null)
        {
            return;
        }

        newWidth = Mathf.Clamp(newWidth, 0f, 10f);
        outline.OutlineWidth = newWidth;
    }

    public void ChangeColor(Color newColor)
    {
        if (outline == null)
        {
            return;
        }

        if(outline.OutlineColor == newColor)
        {
            return;
        }

        outline.OutlineColor = newColor;
    }


    public void ChangeSettings(Outline.Mode newMode, float newWidth, Color newColor)
    {
        if (outline == null)
        {
            return;
        }

        ChangeMode(newMode);
        ChangeWidth(newWidth);
        ChangeColor(newColor);
    }
}
