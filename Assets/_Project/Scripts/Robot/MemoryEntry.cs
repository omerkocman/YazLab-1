using System;
using UnityEngine;

[Serializable]
public class MemoryEntry
{
    public Interactable Interactable;
    public InteractableType InteractableType;
    public Vector3 LastSeenPosition;
    public float LastSeenTime;
    public bool IsInteracted;

    public MemoryEntry(Interactable interactable, Vector3 lastSeenPosition)
    {
        Interactable = interactable;
        InteractableType = interactable.type;
        LastSeenPosition = lastSeenPosition;
        LastSeenTime = Time.time;
        IsInteracted = false;
    }
}
