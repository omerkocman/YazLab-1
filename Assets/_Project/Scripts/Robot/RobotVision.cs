using TMPro;
using UnityEngine;

public class RobotVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField, Range(1, 180)] private float fovAngle = 90f;
    [SerializeField, Range(2, 90)] private int raycastCount = 11;
    [SerializeField, Range(0, 30)] private float maxDistance = 10f;
    [SerializeField] private Transform robotCamera;

    [Header("Layers")]
    [SerializeField] private LayerMask visionLayer;

    public Vector3 BestDirection { get; private set; }
    public Transform BestInteractable { get; private set; }
    public bool HasTarget { get; private set; }


    private void Start()
    {
    }

    private void Update()
    {
        //Look();
    }

    public void CastVisionRays()
    {
        Vector3 origin = robotCamera.position;

        float halfFov = fovAngle * 0.5f;
        float angleStep = fovAngle / (raycastCount - 1);


        float bestScore = float.MinValue;
        Vector3 selectedDirection = transform.forward;
        float selectedDistance = maxDistance;

        for (float currentAngle = -halfFov; currentAngle <= halfFov; currentAngle += angleStep)
        {
            Quaternion currentRotation = Quaternion.Euler(0f, currentAngle, 0f);
            Vector3 currentDirection = currentRotation * transform.forward;

            float currentDistance = maxDistance;

            bool hitInteractable = false;
            bool hitObstacle = false;


            // If hit obstacle layer or interactable layer.
            if (Physics.Raycast(origin, currentDirection, out RaycastHit hit, maxDistance, visionLayer))
            {
                currentDistance = hit.distance;

                Interactable interactable = hit.collider.GetComponentInParent<Interactable>();

                // If hit to Interactable.
                if(interactable)
                {
                    hitInteractable = true;

                    if(BestInteractable == null || hit.distance < Vector3.Distance(transform.position, BestInteractable.position))
                    {
                        BestInteractable = interactable.transform;
                    }
                }

                // If hit to Obstacle layer.
                else
                {
                    hitObstacle = true;
                }

            }

            // If not hit.
            else
            {
                
            }

            // calculate point.

            float finalScore = CalculatePoint(halfFov, currentAngle, currentDistance, hitInteractable, hitObstacle);

            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                selectedDirection = currentDirection;
                selectedDistance = currentDistance;
            }





            //Debug.DrawRay(origin, currentDirection * currentDistance);
        }

        BestDirection = selectedDirection;
        // has clear path if score > 0.2f;

        //Debug.DrawRay(origin, BestDirection * selectedDistance, Color.green);



    }

    private float CalculatePoint(float halfFov, float currentAngle, float currentDistance, bool hitInteractable, bool hitObstacle)
    {
        float angleNormalized = Mathf.Abs(currentAngle) / halfFov;
        float angleScore = Mathf.Lerp(1.0f, 0.2f, angleNormalized);

        float distanceScore = currentDistance / maxDistance;

        if(hitInteractable)
        {
            distanceScore = maxDistance / Mathf.Max(currentDistance, 0.5f);
        }

        float finalScore = (distanceScore * 0.7f) + (angleScore * 0.3f);

        if(hitObstacle && currentDistance < 1.2f)
        {
            finalScore = -1f;
        }

        return finalScore;
    }
}
