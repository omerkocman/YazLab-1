using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls input;

    public Vector2 Move => input.Player.Move.ReadValue<Vector2>();
    public Vector2 Look => input.Player.Look.ReadValue<Vector2>();
    public bool Sprint => input.Player.Sprint.IsPressed();
    public bool Interact => input.Player.Interact.WasPressedThisFrame();

    private void Awake()
    {
        input = new PlayerControls();
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void OnDestroy()
    {
        input?.Dispose();
    }
}
