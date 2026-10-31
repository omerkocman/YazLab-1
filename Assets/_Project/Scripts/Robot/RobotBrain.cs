using DG.Tweening;
using System;
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
    [Header("Target Interaction")]
    [SerializeField, Min(0.1f)] private float interactionDistance = 2f;
    [SerializeField, Min(1f)] private float targetMoveTimeout = 20f;


    public RobotState CurrentState { get; private set; } = RobotState.Idle;
    public RobotTaskType CurrentTask { get; private set; } = RobotTaskType.None;


    public bool IsActivated { get; private set; }

    private RobotNavigation navigation;
    private RobotSensor sensor;
    private RobotExplorer explorer;

    private Switch targetSwitch;
    private RobotDetectable targetDetectable;
    private bool switchInteractionStarted;
    private float targetMoveStartedAt;

    private void Awake()
    {
        navigation = GetComponent<RobotNavigation>();
        explorer = GetComponent<RobotExplorer>();
        sensor = GetComponent<RobotSensor>();
    }

    private void Update()
    {
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
        }
    }

    private void HandleSearching()
    {
        sensor.Scan();

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
                SetState(RobotState.Completed);
                return;
            }

            targetSwitch = foundSwitch;
            targetDetectable = detected;

            if(Vector3.Distance(transform.position, targetSwitch.transform.position) <= interactionDistance)
            {
                SetState(RobotState.Interacting);
                return;
            }

            if(navigation.TryMoveNear(targetSwitch.InteractionPoint))
            {
                targetMoveStartedAt = Time.time;
                SetState(RobotState.MovingToTarget);
                return;
            }

            else
            {
                Debug.Log("[ROBOT BRAIN] Switch detected but no reachable approach point!");
            }

            targetSwitch = null;
            targetDetectable = null;

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
        SetState(RobotState.MovingToExplorePoint);
    }

    private void HandleMovingToExploringPoint()
    {
        if(!navigation.HasReachedDestination())
        {
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
