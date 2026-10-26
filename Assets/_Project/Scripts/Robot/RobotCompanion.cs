using UnityEngine;


enum RobotState
{
    Broken,
    Booting,
    Idle,
    TaskExecuting
}
[SelectionBase]
[RequireComponent(typeof(RobotMovement))]
[RequireComponent(typeof(RobotVision))]
[RequireComponent(typeof(TargetScanner))]
[RequireComponent(typeof(InteractableMemory))]
public class RobotCompanion : Interactable
{
    [Header("Robot Settings")]
    [SerializeField] private RobotState state = RobotState.Idle;
    [SerializeField] private RobotMovement movement;
    [SerializeField] private RobotVision vision;
    [SerializeField] private TargetScanner scanner;
    [SerializeField] private InteractableMemory memory;
    //[SerializeField] private Transform gameObject;

    [Header("Current Task")]
    [SerializeField] private MemoryEntry currentTarget;

    protected override void Awake()
    {
        base.Awake();

        movement = GetComponent<RobotMovement>();
        vision = GetComponent<RobotVision>();
        scanner = GetComponent<TargetScanner>();
        memory = GetComponent<InteractableMemory>();
    }

    private void Update()
    {
       if(state == RobotState.Broken || state == RobotState.Booting)
        {
            return;
        }

        scanner.ScanArea(memory);

        // IF CURRENT TARGET == NULL -> SELECT BEST TARGET!
        if(currentTarget == null || currentTarget.Interactable == null)
        {
            currentTarget = GetBestTarget();

            if(currentTarget != null)
            {
                state = RobotState.TaskExecuting;
                movement.MoveTo(currentTarget.Interactable.transform.position);
            }

            else
            {
                state = RobotState.Idle;

                //if(Vector3.Distance(transform.position, fallbackPosition) > 1.5f)
                //{
                //    movement.MoveTo(fallbackPosition);
                //}
            }
        }

        // TASK EXEC?
        else if(state == RobotState.TaskExecuting)
        {
            // REACHED TO DEST?
            if(movement.HasReachedDestination())
            {
                ExecuteInteraction();
            }
        }
    }

    private MemoryEntry GetBestTarget()
    {
        var targets = memory.MemoryEntries;

        if(targets == null || targets.Count == 0)
        {
            return null;
        }

        MemoryEntry best = null;
        float bestScore = float.MinValue;

        foreach(var target in targets)
        {
            if(target == null)
            {
                continue;
            }

            if(target.IsInteracted)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, target.Interactable.transform.position);
            float score = 0f;

            if (target.InteractableType == InteractableType.Door)
            {
                score = 200f - distance;
            }

            if(score > bestScore)
            {
                bestScore = score;
                best = target;
            }

        }

        return best;
    }

    private void ExecuteInteraction()
    {
        movement.Stop();
        
        currentTarget.Interactable.Interact(null);

        memory.MarkAsInteracted(currentTarget.Interactable);

        currentTarget = null;

        state = RobotState.Idle;
    }

    public override void Interact(Player player) 
    {

    }
}
