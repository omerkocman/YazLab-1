using UnityEngine;

public class Pickable : Interactable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;
    private string propmt = "Pickup";

    protected override void Awake()
    {
        base.Awake();
        SetPropmtMessage(propmt);
    }
    
    public override void Interact()
    {
        
    }
}

