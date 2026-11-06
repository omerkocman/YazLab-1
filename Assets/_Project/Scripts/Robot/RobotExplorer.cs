using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(RobotSensor))]
[RequireComponent(typeof(RobotNavigation))]
public class RobotExplorer : MonoBehaviour
{
    [Header("Exploration Settings")]
    [SerializeField, Min(0.5f)] private float explorationStep = 3f;
    [SerializeField, Min(0.1f)] private float obstacleClearance = 0.8f;
    [SerializeField, Min(0.1f)] private float minTravelDistance = 1.2f;
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 0.5f;
    [SerializeField, Min(0.1f)] private float visitedRadius = 1.5f;

    [SerializeField, Min(0.1f)] private float frontierMergeRadius = 0.8f;

    private RobotSensor sensor;
    private RobotNavigation navigation;

    private readonly List<Vector3> visitedPoints = new List<Vector3>();

    private readonly List<Vector3> frontierPoints = new List<Vector3>();
    private readonly HashSet<Vector3> localFrontiers = new HashSet<Vector3>();
    private Vector3? selectedPoint;

    private void Awake()
    {
        sensor = GetComponent<RobotSensor>();
        navigation = GetComponent<RobotNavigation>();
    }

    public bool TryGetNextPoint(out Vector3 nextPoint)
    {
        nextPoint = Vector3.zero;
        selectedPoint = null;

        if(visitedPoints.Count == 0)
        {
            MarkVisited(transform.position);
        }

        sensor.ScanEnvironment();
        localFrontiers.Clear();

        foreach(EnvironmentRayReading ray in sensor.EnvironmentReadings)
        {
            float travelDistance = ray.HasHit ? Mathf.Min(explorationStep, ray.Distance - obstacleClearance) : explorationStep;

            if(travelDistance < minTravelDistance)
            {
                continue;
            }

            Vector3 rawPoint = transform.position + ray.Direction * travelDistance;

            if(!NavMesh.SamplePosition(rawPoint, out NavMeshHit navHit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                continue;
            }

            Vector3 candidate = navHit.position;

            if(Vector3.Distance(transform.position, candidate) < minTravelDistance)
            {
                continue;
            }

            AddFrontier(candidate);
        }

        frontierPoints.RemoveAll(HasVisited);

        int bestIndex = -1;
        float bestScore = float.NegativeInfinity;
        bool bestIsLocal = false;
        float localPathLimit = explorationStep + navMeshSampleRadius + frontierMergeRadius;
        Vector3 origin = transform.position;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        for(int i = frontierPoints.Count - 1; i >= 0; i--)
        {
            Vector3 candidate = frontierPoints[i];

            if (!navigation.TryGetPathLength(candidate, out float travelDistance))
            {
                localFrontiers.Remove(candidate);
                continue;
            }

            bool isLocal = localFrontiers.Contains(candidate) && travelDistance <= localPathLimit;
            if (!isLocal)
            {
                localFrontiers.Remove(candidate);
            }

            float progress = isLocal ? Vector3.Dot(candidate - origin, forward) : GetDistanceFromVisited(candidate);
            float score = progress - travelDistance * 0.15f;

            if (bestIndex == -1 || (isLocal && !bestIsLocal) || (isLocal == bestIsLocal && score > bestScore))
            {
                bestScore = score;
                bestIndex = i;
                bestIsLocal = isLocal;
            }

        }

        Debug.Log($"[ROBOT EXPLORER] Pending: {frontierPoints.Count}, " + $"Visited: {visitedPoints.Count}");

        if (bestIndex == -1)
        {
            return false;

        }

        nextPoint = frontierPoints[bestIndex];
        selectedPoint = nextPoint;
        frontierPoints.RemoveAt(bestIndex);

        return true;

    }

    [ContextMenu("Test Next Exploration Point")]
    private void TestNextExplorationPoint()
    {
        if(!TryGetNextPoint(out Vector3 point))
        {
            Debug.LogWarning("[ROBOT EXPLORER] No exploration point found!");
            return;
        }

        Debug.DrawLine(transform.position + Vector3.up, point + Vector3.up, Color.magenta, 5f);

        Debug.Log($"[ROBOT EXPLORER] Selected point: {point}");
    }

    private float GetDistanceFromVisited(Vector3 position)
    {
        float minDistance = float.PositiveInfinity;

        foreach(Vector3 visited in visitedPoints)
        {
            float distance = Vector3.Distance(position, visited);

            if(distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    public bool HasVisited(Vector3 position)
    {
        foreach(Vector3 visited in visitedPoints)
        {
            if(Vector3.Distance(position, visited) <= visitedRadius)
            {
                return true;
            }
        }

        return false;
    }

    public void MarkVisited(Vector3 position)
    {
        selectedPoint = null;
        if(!HasVisited(position))
        {
            visitedPoints.Add(position);
        }

        frontierPoints.RemoveAll(point => Vector3.Distance(point, position) <= visitedRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Color previousColor = Gizmos.color;
        Vector3 markerOffset = Vector3.up * 0.2f;

        Gizmos.color = new Color(0.25f, 0.65f, 1f);
        foreach (Vector3 point in visitedPoints)
        {
            Gizmos.DrawSphere(point + markerOffset, 0.12f);
            Gizmos.DrawWireSphere(point, visitedRadius);
        }

        foreach (Vector3 point in frontierPoints)
        {
            Gizmos.color = localFrontiers.Contains(point) ? Color.green : Color.yellow;
            Gizmos.DrawSphere(point + markerOffset, 0.16f);
        }

        if (selectedPoint.HasValue)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(selectedPoint.Value + markerOffset, 0.25f);
        }

        Gizmos.color = previousColor;
    }

    private void AddFrontier(Vector3 point)
    {
        if(HasVisited(point))
        {
            return;
        }

        foreach(Vector3 existing in frontierPoints)
        {
            if(Vector3.Distance(existing, point) <= frontierMergeRadius)
            {
                localFrontiers.Add(existing);
                return;
            }
        }

        frontierPoints.Add(point);
        localFrontiers.Add(point);

    }

}
