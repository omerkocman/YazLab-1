using System;
using UnityEngine;

[SelectionBase]
public class NPCGuardInteractable : InteractableBase
{
    [Header("NPC Guard Settings")]
    [SerializeField] private string guardName = "Security Guard";
    
    public event Action<GameObject> ConversationRequested;

    public override string GetInteractionPrompt()
    {
        return $"Talk to {guardName}";
    }

    public override void Interact(GameObject interactor)
    {
        if(!CanInteract(interactor))
        {
            return;
        }

        Debug.Log($"[NPC GUARD] Interaction requested by {interactor.name}");

        ConversationRequested?.Invoke(interactor);
    }
}
