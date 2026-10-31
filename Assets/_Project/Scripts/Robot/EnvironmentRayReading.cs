using UnityEngine;

public readonly struct EnvironmentRayReading
{
    public Vector3 Direction { get; }
    public float Distance { get; }
    public bool HasHit { get; }
    public Vector3 EndPoint { get; }

    public EnvironmentRayReading(Vector3 direction, float distance, bool hasHit, Vector3 endPoint)
    {
        Direction = direction;
        Distance = distance;
        HasHit = hasHit;
        EndPoint = endPoint;
    }
}
