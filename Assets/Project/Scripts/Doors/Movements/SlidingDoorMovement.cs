using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SlidingDoorStrategy : IDoorMovementStrategy
{
    [Range(0,100)]
    public int openPercentage = 5;
    public bool reverse = false;

    private Vector3 _initialPosition;
    private bool _isInitialized = false;
    private float _meshDimension = -1f;

    public void Animate(Transform doorMesh, bool isOpen, float speed)
    {
        if(!_isInitialized)
        {
            Initalize(doorMesh);
        }

        float travelDistance = _meshDimension * (1 - openPercentage * 0.01f);
        if(reverse)
        {
            travelDistance = -travelDistance;
        }

        Vector3 openPosition = _initialPosition + new Vector3(0f, 0f, -travelDistance);
        Vector3 targetPosition = isOpen ? openPosition : _initialPosition;

        doorMesh.localPosition = Vector3.Lerp(doorMesh.localPosition, targetPosition, Time.deltaTime * speed);
    }

    private void Initalize(Transform doorMesh)
    {
        InitializePosition(doorMesh);
        CalculateDimension(doorMesh);
    }

    public void ResetState()
    {
        _isInitialized = false;
    }

    private void InitializePosition(Transform doorMesh)
    {
        _initialPosition = doorMesh.localPosition;
        _isInitialized = true;
    }

    private void CalculateDimension(Transform doorMesh)
    {
        Renderer r = doorMesh.GetComponentInChildren<Renderer>();
        if(r != null)
        {
            _meshDimension = Mathf.Max(r.bounds.size.x, r.bounds.size.z);
        }

        else
        {
            _meshDimension = 2f;
        }
    }
}
