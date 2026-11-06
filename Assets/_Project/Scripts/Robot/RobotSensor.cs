using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobotSensor : MonoBehaviour
{
    [Header("Sensor Settings")]
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField, Range(1f, 360f)] private float fieldOfView = 120f;

    [Header("Layers")]
    [SerializeField] private LayerMask detectionMask;
    [SerializeField] private LayerMask visibilityMask = ~0;

    [Header("Environment Scanning")]
    [SerializeField, Min(3)] private int environmentRayCount = 13;
    [SerializeField, Range(10f, 360f)] private float environmentScanAngle = 180f;
    [SerializeField, Min(0.1f)] private float environmentScanDistance = 8f;
    [SerializeField] private LayerMask environmentMask;

    // Environment Readings
    private readonly List<EnvironmentRayReading> environmentReadings = new List<EnvironmentRayReading>();

    public IReadOnlyList<EnvironmentRayReading> EnvironmentReadings => environmentReadings;

    // Visible Objects
    private readonly Collider[] results = new Collider[64];
    private readonly List<RobotDetectable> visibleObjects = new List<RobotDetectable>();

    public IReadOnlyList<RobotDetectable> VisibleObjects => visibleObjects;

    [ContextMenu("Test Scan")]
    private void TestScan() => Scan(8f);

    public void Scan() => Scan(0f);

    private void Scan(float debugDuration)
    {
        visibleObjects.Clear();

        if(sensorOrigin == null)
        {
            Debug.LogError("[ROBOT SENSOR] SensorOrigin is null!");
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(sensorOrigin.position, detectionRange, results, detectionMask, QueryTriggerInteraction.Ignore);

        //Debug.Log($"[ROBOT SENSOR] Detected colliders: {count}");

        for(int i = 0; i < count; i++)
        {
            Collider candidate = results[i];

            RobotDetectable target = candidate.GetComponentInParent<RobotDetectable>();

            if(target == null || target.transform.IsChildOf(transform) || visibleObjects.Contains(target))
            {
                continue;
            }

            Vector3 direction = candidate.bounds.center - sensorOrigin.position;

            float distance = direction.magnitude;

            if(distance < 0.01f)
            {
                continue;
            }

            float angle = Vector3.Angle(sensorOrigin.forward, direction);

            if(angle > fieldOfView /2)
            {
                continue;
            }

            bool hitSomething = Physics.Raycast(sensorOrigin.position, direction.normalized, out RaycastHit hit, distance + 0.05f, visibilityMask, QueryTriggerInteraction.Ignore);

            if(!hitSomething)
            {
                continue;
            }

            if (hitSomething)
            {
                bool isTarget =
                    hit.collider.GetComponentInParent<RobotDetectable>() == target;

                Debug.DrawLine(
                    sensorOrigin.position,
                    hit.point,
                    isTarget ? Color.green : Color.red,
                    debugDuration
                );

                //Debug.Log($"[ROBOT SENSOR] Raycast hit: {hit.collider.name}");
            }

            RobotDetectable seenObject = hit.collider.GetComponentInParent<RobotDetectable>();

            if(seenObject != target)
            {
                continue;
            }

            visibleObjects.Add(target);

            Debug.Log($"[ROBOT SENSOR] Saw: {target.name} ({target.Type})");
        }

    }

    [ContextMenu("Test Environment Scan")]
    private void TestEnvironmentScan() => ScanEnvironment(8f);

    public void ScanEnvironment() => ScanEnvironment(0.1f);

    private void ScanEnvironment(float debugDuration)
    {
        environmentReadings.Clear();

        if(sensorOrigin == null)
        {
            Debug.LogError("[ROBOT SENSOR] SensorOrigin is null!");
            return;
        }

        Vector3 origin = sensorOrigin.position;

        Vector3 forward = Vector3.ProjectOnPlane(sensorOrigin.forward, Vector3.up).normalized;

        if(forward.sqrMagnitude < 0.001f)
        {
            return;
        }

        int rayCount = Mathf.Max(3, environmentRayCount);

        int hitCount = 0;

        for(int i = 0; i < rayCount; i++)
        {
            float t = environmentScanAngle >= 359.9f
            ? i / (float)rayCount
            : i / (float)(rayCount - 1);

            float angle = Mathf.Lerp(-environmentScanAngle / 2f, environmentScanAngle / 2f, t);

            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;

            bool hasHit = Physics.Raycast(origin, direction, out RaycastHit hit, environmentScanDistance, environmentMask, QueryTriggerInteraction.Ignore);

            float distance = hasHit ? hit.distance : environmentScanDistance;

            Vector3 endPoint = hasHit ? hit.point : origin + direction * environmentScanDistance;

            environmentReadings.Add(new EnvironmentRayReading(
                direction,
                distance,
                hasHit,
                endPoint
            ));

            if(hasHit)
            {
                hitCount++;
            }

            Debug.DrawLine(origin, endPoint, hasHit ? Color.red : Color.green, debugDuration);
        }

        Debug.Log($"[ROBOT SENSOR] Environment: {rayCount} rays, {hitCount} hits.");
    }

    private void OnDrawGizmosSelected()
    {
        if(sensorOrigin == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(sensorOrigin.position, detectionRange);

        Gizmos.DrawRay(sensorOrigin.position, sensorOrigin.forward * detectionRange);
    }

}
