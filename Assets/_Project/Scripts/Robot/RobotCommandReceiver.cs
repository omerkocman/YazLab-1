using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(RobotBrain))]
public class RobotCommandReceiver : InteractableBase
{
    private RobotBrain brain;

    protected override void Awake()
    {
        base.Awake();

        brain = GetComponent<RobotBrain>();
    }

    public override void Interact(GameObject interactor)
    {
        if(!CanInteract(interactor))
        {
            return;
        }

        brain.StartTask(RobotTaskType.DisableGas);
    }

    public override bool CanInteract(GameObject interactor)
    {
        return brain != null && !brain.IsActivated;
    }

    public override string GetInteractionPrompt()
    {
        return brain != null && brain.IsActivated ? "Robot is working..." : "Activate Robot";
    }

    public override string GetUnavailableMessage(GameObject interactor)
    {
        return "Robot is alredy working.";
    }
}
