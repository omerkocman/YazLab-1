using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RobotNavigation : MonoBehaviour
{
    private NavMeshAgent agent;
    private NavMeshPath path;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }

    public bool CanReach(Vector3 destination)
    {
        if(!agent.isOnNavMesh)
        {
            return false;
        }

        bool found = agent.CalculatePath(destination, path);

        return found && path.status == NavMeshPathStatus.PathComplete;
    }

    public bool TryMoveTo(Vector3 destination)
    {
        if(!CanReach(destination))
        {
            return false;
        }

        agent.isStopped = false;

        return agent.SetPath(path);
    }

    public bool TryMoveNear(Vector3 position, float sampleRadius = 1.5f)
    {
        if(!agent.isOnNavMesh)
        {
            return false;
        }

        if(!NavMesh.SamplePosition(position, out NavMeshHit hit, sampleRadius, agent.areaMask))
        {
            return false;
        }

        return TryMoveTo(hit.position);
    }
    
    public bool HasReachedDestination()
    {
        if(!agent.isOnNavMesh || agent.pathPending)
        {
            return false;
        }

        if(!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        return !float.IsInfinity(agent.remainingDistance) && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
    }

    public void Stop()
    {
        if(!agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }
}
