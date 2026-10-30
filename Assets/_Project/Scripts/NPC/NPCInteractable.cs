using UnityEngine;

public class NPCInteractable : Interactable
{
    private NPCBrain brain;

    private void Start()
    {
        brain = GetComponent<NPCBrain>();
    }

    public override void Interact(Player player)
    {
        if (brain != null)
        {
            brain.StartConversation();
        }
        else
        {
            Debug.LogError("Bu objede NPCBrain bileþeni bulunamadý!");
        }
    }
}