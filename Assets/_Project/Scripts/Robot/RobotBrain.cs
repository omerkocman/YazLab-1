using System.Linq;
using UnityEngine;

public enum RobotState
{
    Idle,
    Searching,
    Exploring,
    MovingToExplorePoint,
    MovingToTarget,
    Interacting,
    MovingToDoor,
    OpeningDoor,
    Completed,
    Failed
}

public enum RobotTaskType
{
    None,
    DisableGas
}

[RequireComponent(typeof(RobotNavigation))]
[RequireComponent(typeof(RobotExplorer))]
[RequireComponent(typeof(RobotSensor))]

public class RobotBrain : MonoBehaviour
{
    private const float SwitchScanInterval = 0.3f;

    [Header("Target Interaction")]
    [SerializeField, Min(0.1f)] private float interactionDistance = 2f;
    [SerializeField, Min(1f)] private float targetMoveTimeout = 20f;

    [SerializeField] private Transform testDestination;

    public RobotState CurrentState { get; private set; } = RobotState.Idle;
    public RobotTaskType CurrentTask { get; private set; } = RobotTaskType.None;


    public bool IsActivated { get; private set; }

    private RobotNavigation navigation;
    private RobotSensor sensor;
    private RobotExplorer explorer;

    // Switch
    private Switch targetSwitch;
    private RobotDetectable targetDetectable;
    private bool switchInteractionStarted;
    private float targetMoveStartedAt;
    private float nextSwitchScanAt;

    // Door
    private DoorInteractable targetDoor;
    private bool doorOpenRequested;
    private float doorMoveStartedAt;
    private Vector3 targetDoorApproachPosition;

    private void Awake()
    {
        navigation = GetComponent<RobotNavigation>();
        explorer = GetComponent<RobotExplorer>();
        sensor = GetComponent<RobotSensor>();
    }

    private void Update()
    {
        //if(testDestination != null && CurrentState != RobotState.Idle && CurrentState != RobotState.Failed && CurrentState != RobotState.Completed)
        //{
            
        //    if(!navigation.TryMoveTo(testDestination.position))
        //    {
        //        Debug.Log("[ROBOT BRAIN] Cant reach to TestDestination!");
        //        SetState(RobotState.Failed);
        //        return;

        //    }

        //    Debug.Log("[ROBOT BRAIN] Reached to TestDestination!");
        //    SetState(RobotState.Completed);
        //    return;
        //}

        switch(CurrentState)
        {
            case RobotState.Searching:
                HandleSearching();
                break;

            case RobotState.Exploring:
                HandleExploring();
                break;

            case RobotState.MovingToExplorePoint:
                HandleMovingToExploringPoint();
                break;

            case RobotState.MovingToTarget:
                HandleMovingToTarget();
                break;

            case RobotState.Interacting:
                HandleInteracting();
                break;

            case RobotState.MovingToDoor:
                HandleMovingToDoor();
                break;

            case RobotState.OpeningDoor:
                HandleOpeningDoor();
                break;

        }
    }

    private bool TryApproachVisibleSwitch()
    {
        foreach(RobotDetectable detected in sensor.VisibleObjects)
        {
            if(CurrentTask != RobotTaskType.DisableGas || detected.Type != RobotObjectType.Switch)
            {
                continue;
            }

            Switch foundSwitch = detected.GetComponent<Switch>();

            if(foundSwitch == null)
            {
                continue;
            }

            if(!foundSwitch.IsOpen)
            {
                navigation.Stop();
                SetState(RobotState.Completed);
                return true;
            }

            targetSwitch = foundSwitch;
            targetDetectable = detected;

            if(Vector3.Distance(transform.position, targetSwitch.transform.position) <= interactionDistance)
            {
                navigation.Stop();
                SetState(RobotState.Interacting);
                return true;
            }

            if(navigation.TryMoveNear(targetSwitch.InteractionPosition))
            {
                targetMoveStartedAt = Time.time;
                SetState(RobotState.MovingToTarget);
                return true;
            }

            Debug.Log("[ROBOT BRAIN] Switch detected but no reachable approach point!");
            targetSwitch = null;
            targetDetectable = null;
        }

        return false;
    }

    private void HandleSearching()
    {
        sensor.Scan();
        if (TryApproachVisibleSwitch())
        {
            return;
        }

        foreach(RobotDetectable detected in sensor.VisibleObjects)
        {
            if(detected.Type != RobotObjectType.Door)
            {
                continue;
            }

            DoorInteractable door = detected.GetComponent<DoorInteractable>();

            if(door == null || door.IsOpen || door.IsMoving)
            {
                continue;
            }

            Transform a = door.InteractionPositionA;
            Transform b = door.InteractionPositionB;

            if(a != null && b != null)
            {
                float distanceA = Vector3.Distance(transform.position, a.position);
                float distanceB = Vector3.Distance(transform.position, b.position);

                if(distanceB < distanceA)
                {
                    (a, b) = (b, a);
                }
            }

            if(TryApproachDoor(door, a))
            {
                return;
            }

            if(TryApproachDoor(door, b))
            {
                return;
            }

        }

        SetState(RobotState.Exploring);

    }

    private void HandleExploring()
    {
        if(!explorer.TryGetNextPoint(out Vector3 point))
        {
            SetState(RobotState.Failed);
            return;
        }

        if(!navigation.TryMoveTo(point))
        {
            SetState(RobotState.Failed);
            return;
        }

        Debug.Log($"[ROBOT BRAIN] Exploring point: {point}");
        nextSwitchScanAt = Time.time + SwitchScanInterval;
        SetState(RobotState.MovingToExplorePoint);
    }

    private void HandleMovingToExploringPoint()
    {
        if(!navigation.HasReachedDestination())
        {
            if (Time.time >= nextSwitchScanAt)
            {
                nextSwitchScanAt = Time.time + SwitchScanInterval;
                sensor.Scan();
                if (TryApproachVisibleSwitch())
                {
                    explorer.MarkVisited(transform.position);
                }
            }

            return;
        }

        navigation.Stop();

        explorer.MarkVisited(transform.position);

        SetState(RobotState.Searching);
    }

    private void HandleMovingToTarget()
    {
        if(targetSwitch == null)
        {
            navigation.Stop();
            SetState(RobotState.Failed);
            return;
        
        }

        if(Time.time - targetMoveStartedAt > targetMoveTimeout)
        {
            navigation.Stop();

            Debug.LogWarning("[ROBOT BRAIN] Target movement timed out.");
            SetState(RobotState.Failed);
            return;
        
        }

        if(!navigation.HasReachedDestination())
        {
            return;
        }

        navigation.Stop();

        SetState(RobotState.Interacting);

    }
    private void HandleInteracting()
    {
        if(targetSwitch == null || targetDetectable == null)
        {
            SetState(RobotState.Failed);
            return;
        }

        if(switchInteractionStarted)
        {
            if(targetSwitch.IsMoving)
            {
                return;
            }

            switchInteractionStarted = false;

            SetState(!targetSwitch.IsOpen ? RobotState.Completed : RobotState.Failed);
            return;
        }

        float distance = Vector3.Distance(transform.position, targetSwitch.transform.position);

        if(distance > interactionDistance)
        {
            Debug.LogWarning("[ROBOT BRAIN] Switch is too far!");
            SetState(RobotState.Failed);
            return;
        }

        Vector3 direction = targetSwitch.transform.position - transform.position;

        direction.y = 0f;

        if(direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);    
        }

        sensor.Scan();

        if(!sensor.VisibleObjects.Contains(targetDetectable))
        {
            Debug.LogWarning("[ROBOT BRAIN] Switch is not visible!");
            SetState(RobotState.Failed);
            return;
        }

        if(targetSwitch.IsOpen && targetSwitch.CanInteract(gameObject))
        {
            switchInteractionStarted = true;
            targetSwitch.Interact(gameObject);
        }

        else
        {
            SetState(!targetSwitch.IsOpen ? RobotState.Completed : RobotState.Failed);

        }

    }

    private void HandleMovingToDoor()
    {
        if(targetDoor == null)
        {
            SetState(RobotState.Failed);
            return;
        }

        if(Time.time - doorMoveStartedAt > targetMoveTimeout)
        {
            navigation.Stop();

            Debug.Log("[ROBOT BRAIN] Could not reach door.");

            targetDoor = null;
            doorOpenRequested = false;

            SetState(RobotState.Exploring);
            return;
        }

        if(!navigation.HasReachedDestination())
        {
            return;
        }

        navigation.Stop();

        doorOpenRequested = false;

        SetState(RobotState.OpeningDoor);
    }

    private void HandleOpeningDoor()
    {
        if(targetDoor == null)
        {
            SetState(RobotState.Failed);
            return;
        }

        if(targetDoor.IsOpen)
        {
            Debug.Log("[ROBOT BRAIN] Door opened successfully");

            targetDoor = null;
            doorOpenRequested = false;

            SetState(RobotState.Searching);
            return;
        }

        if(doorOpenRequested)
        {
            if(targetDoor.IsMoving)
            {
                return;
            }

            Debug.LogWarning("[ROBOT BRAIN] Door could not be opened.");

            targetDoor = null;
            doorOpenRequested = false;

            
            SetState(RobotState.Exploring);
            return;
        }

        float distance = Vector3.Distance(transform.position, targetDoorApproachPosition);


        if(distance > 1f)
        {
            Debug.LogWarning("[ROBOT BRAIN] Too far from door.");

            targetDoor = null;

            SetState(RobotState.Exploring);
            return;
        }

        if (!targetDoor.CanInteract(gameObject))
        {
            targetDoor = null;
            SetState(RobotState.Exploring);
            return;
        }

        doorOpenRequested = true;

        targetDoor.Interact(gameObject);
    }

    private bool TryApproachDoor(DoorInteractable door, Transform point)
    {
        if(point == null)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, point.position);

        if(distance <= 0.85f && navigation.CanReach(point.position))
        {
            targetDoor = door;
            targetDoorApproachPosition = point.position;
            doorOpenRequested = false;

            SetState(RobotState.OpeningDoor);
            return true;
        }

        if(!navigation.TryMoveNear(point.position, 0.4f))
        {
            return false;
        }

        targetDoor = door;
        targetDoorApproachPosition = point.position;
        doorMoveStartedAt = Time.time;
        doorOpenRequested = false;

        SetState(RobotState.MovingToDoor);
        return true;

    }

    public bool StartTask(RobotTaskType task)
    {
        if(IsActivated || task == RobotTaskType.None)
        {
            return false;
        }

        CurrentTask = task;
        IsActivated = true;

        SetState(RobotState.Searching);

        return true;
    }

    private void SetState(RobotState newState)
    {
        if(CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;

        Debug.Log($"[ROBOT BRAIN] State: {newState}");
    }
}
