using System;
using UnityEngine;

public class InteractableOutline
{
    // Outline Settings
    private Color outlineColor = Color.white;
    private float outlineWidth = 10f;
    private Outline _outline;


    public InteractableOutline(GameObject gameObject)
    {
        _outline = gameObject.GetComponentInChildren<Outline>();
        if(_outline != null)
        {
            SetDefaultSettings();
        }
    }
    public void Enable()
    {
        if(_outline != null)
        {
            _outline.enabled = true;
        }
    }

    public void Disable()
    {
        if(_outline != null)
        {
            _outline.enabled = false;
        }
    }

    public void ChangeColor(Color color)
    {
        _outline.OutlineColor = color;
    }

    private void SetDefaultSettings()
    {
        _outline.OutlineMode = Outline.Mode.OutlineVisible;
        _outline.OutlineColor = outlineColor;
        _outline.OutlineWidth = outlineWidth;
        _outline.enabled = false;
    }
}
