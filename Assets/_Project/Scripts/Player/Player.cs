using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(Inventory))]
public class Player : MonoBehaviour
{
    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerInteraction Interaction { get; private set; }
    public Inventory Inventory { get; private set; }

    private void Awake()
    {
        InputHandler = GetComponent<PlayerInputHandler>();
        Movement = GetComponent<PlayerMovement>();
        Interaction = GetComponent<PlayerInteraction>();
        Inventory = GetComponent<Inventory>();
    }
}
