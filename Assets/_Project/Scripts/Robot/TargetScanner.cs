using System.Collections.Generic;
using UnityEngine;

public class TargetScanner : MonoBehaviour
{
    [Header("Scanner Settings")]
    [SerializeField] private float scanRadius = 15f;
    [SerializeField] private Transform robotCamera;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private void Update()
    {
        //ScanArea();
    }

    public void ScanArea(InteractableMemory memory)
    {
        Collider[] colliders = Physics.OverlapSphere(robotCamera.position, scanRadius, interactableLayer);

        foreach (Collider collider in colliders)
        {
            
            if (collider == null)
            {
                continue;
            }

            Interactable interactable = collider.GetComponentInParent<Interactable>();
            
            if(interactable == null)
            {
                continue;
            }

            if(!CanSeeTarget(interactable))
            {
                continue;
            }

            if(interactable.type == InteractableType.Door)
            {
                memory.RecordOrUpdate(interactable, interactable.transform.position);
            }
        }
    }
    private bool CanSeeTarget(Interactable interactable)
    {
        Vector3 origin = robotCamera.position;

        Vector3 targetPosition = interactable.TargetPoint;

        Vector3 directionToTarget = targetPosition - origin;



        float distanceToTarget = directionToTarget.magnitude;

        if (Physics.Raycast(origin, directionToTarget.normalized, out RaycastHit hit, distanceToTarget, obstacleLayer))
        {
            Debug.DrawLine(origin, hit.point, Color.red);

            return false;
        }
        Debug.DrawLine(origin, targetPosition, Color.blue);

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if(robotCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(robotCamera.position, scanRadius);
        }
    }
}
