using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    private void Awake()
    {
        sensor = GetComponent<RobotSensor>();
        navigation = GetComponent<RobotNavigation>();
    }

    public bool TryGetNextPoint(out Vector3 nextPoint)
    {
        nextPoint = Vector3.zero;

        if(visitedPoints.Count == 0)
        {
            MarkVisited(transform.position);
        }

        sensor.ScanEnvironment();


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


        for(int i = frontierPoints.Count - 1; i >= 0; i--)
        {
            Vector3 candidate = frontierPoints[i];

            if (HasVisited(candidate))
            {
                frontierPoints.RemoveAt(i);
                continue;
            }

            if (!navigation.CanReach(candidate))
            {
                continue;
            }

            float novelty = GetDistanceFromVisited(candidate);
            float travelDistance = Vector3.Distance(transform.position, candidate);

            float score = novelty - travelDistance * 0.15f;

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }

        }

        Debug.Log($"[ROBOT EXPLORER] Pending: {frontierPoints.Count}, " + $"Visited: {visitedPoints.Count}");

        if (bestIndex == -1)
        {
            return false;

        }

        nextPoint = frontierPoints[bestIndex];
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
        if(!HasVisited(position))
        {
            visitedPoints.Add(position);
        }

        frontierPoints.RemoveAll(point => Vector3.Distance(point, position) <= visitedRadius);
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
                return;
            }
        }

        frontierPoints.Add(point);

    }

}
