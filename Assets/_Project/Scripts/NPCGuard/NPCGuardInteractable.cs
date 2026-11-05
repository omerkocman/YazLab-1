using System;
using UnityEngine;

[SelectionBase]
public class NPCGuardInteractable : InteractableBase
{
    [Header("NPC Guard Settings")]
    [SerializeField] private string guardName = "NPCGuard";

    public string GuardName => string.IsNullOrWhiteSpace(guardName) ? "NPCGuard" : guardName.Trim();
    
    public event Action<GameObject> ConversationRequested;

    public override string GetInteractionPrompt()
    {
        return $"Talk to {GuardName}";
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
