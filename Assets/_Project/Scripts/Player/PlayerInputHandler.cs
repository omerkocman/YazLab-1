using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls _controls;

    public Vector2 Move => _controls.Player.Move.ReadValue<Vector2>();
    public Vector2 Look => _controls.Player.Look.ReadValue<Vector2>();
    public bool Sprint => _controls.Player.Sprint.IsPressed();
    public bool Jump => _controls.Player.Jump.WasPressedThisFrame();
    public bool Interact => _controls.Player.Interact.WasPressedThisFrame();

    private void Awake()
    {
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
    }

    public string GetInteractionBindingName()
    {
        return _controls.Player.Interact.GetBindingDisplayString();
    }
}
