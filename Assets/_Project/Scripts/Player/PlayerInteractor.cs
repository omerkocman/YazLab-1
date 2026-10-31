using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Player Interactor")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask raycastMask = ~0;

    public event Action<string> PromptChanged;
    public event Action<string> FeedbackRequested;

    private string lastPrompt = "";
    public string CurrentPrompt => currentTarget != null ? currentTarget.GetInteractionPrompt() : "";

    private PlayerInputHandler input;
    private InteractableBase currentTarget;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();

    }
    private void Update()
    {
        UpdateTarget();

        if(currentTarget != null)
        {
            currentTarget.RefreshHighlight(gameObject);
        }
        
        if(input.Interact)
        {
            TryInteract();
        }

        RefreshPrompt();

    }

    private void UpdateTarget()
    {
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hitInfo, interactionDistance, raycastMask))
        {
            InteractableBase interactable = hitInfo.collider.GetComponentInParent<InteractableBase>();

            SetTarget(interactable);
        }

        else
        {
            ClearTarget();
        }
    }

    private void SetTarget(InteractableBase newTarget)
    {
        if(newTarget == currentTarget)
        {
            return;
        }

        if(currentTarget != null)
        {
            currentTarget.SetHighlighted(false, gameObject);
        }

        currentTarget = newTarget;

        if(currentTarget != null)
        {
            currentTarget.SetHighlighted(true, gameObject);
        }
    }

    private void TryInteract()
    {
        if(currentTarget == null)
        {
            return;    
        }

        if(!currentTarget.CanInteract(gameObject))
        {
            FeedbackRequested?.Invoke(currentTarget.GetUnavailableMessage(gameObject));
            return;
        }

        currentTarget.Interact(gameObject);
    }

    private void ClearTarget()
    {
        SetTarget(null);
    }

    private void OnDisable()
    {
        ClearTarget();
        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        string newPrompt = CurrentPrompt;

        if(lastPrompt == newPrompt)
        {
            return;
        }

        lastPrompt = newPrompt;

        PromptChanged?.Invoke(newPrompt);
    }
}
