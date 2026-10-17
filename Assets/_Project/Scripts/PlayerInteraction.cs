using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float playerReach = 3f;
    [SerializeField] private Camera playerCamera;
    private PlayerInputHandler _input;

    private Interactable currentInteractable;

    private void Awake()
    {
        _input = GetComponent<PlayerInputHandler>();

        if(playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        CheckInteractable();

        bool interact = _input.Interact;
        if(interact)
        {
            if(currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
        
    }

    private void CheckInteractable()
    {
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, playerReach))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();

            if(interactable != null)
            {
                if(interactable != currentInteractable)
                {
                    ResetCurrentInteractable();
                    SetCurrentInteractable(interactable);
                }
                // if
                InteractionUI.Instance.Show(_input.GetInteractionBindingName(), currentInteractable.GetPropmt());
                
            }

            else
            {
                ResetCurrentInteractable();
            }
        }

        else
        {
            InteractionUI.Instance.Hide();
            ResetCurrentInteractable();
        }
    }

    private void SetCurrentInteractable(Interactable interactable)
    {
        currentInteractable = interactable;
        currentInteractable.OnFocus();
    }

    private void ResetCurrentInteractable()
    {
        if(currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }
}
