using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RobotMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 destination)
    {
        if(agent.isStopped)
        {
            agent.isStopped = false;
        }

        agent.SetDestination(destination);

    }

    public bool HasReachedDestination()
    {
        if(agent.pathPending)
        {
            return false;
        }

        if(!agent.hasPath)
        {
            return false;
        }

        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            if(!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
        
            }
        
        }

        return false;
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }


    // Explore...
}