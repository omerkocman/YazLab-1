using System;
using UnityEngine;

public class InteractableOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0, 10)] private float outlineWidth = 10f;
    private Outline _outline;

    private void Awake()
    {
        _outline = GetComponentInChildren<Outline>();
        
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

    private void SetDefaultSettings()
    {
        _outline.OutlineMode = Outline.Mode.OutlineVisible;
        _outline.OutlineColor = outlineColor;
        _outline.OutlineWidth = outlineWidth;
        _outline.enabled = false;
    }
}
