using UnityEngine;

public interface IInteractable
{
    bool CanInteract(GameObject interactor);
    string GetInteractionPrompt();
    void Interact(GameObject interactor);
}
