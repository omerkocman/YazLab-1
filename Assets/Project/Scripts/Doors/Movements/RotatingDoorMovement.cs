using System;
using UnityEngine;

[Serializable]
public class RotatingDoorMovement : IDoorMovementStrategy
{
    [Range(0, 180)]
    public float openAngle = 90f;
    public bool reverse = false;

    
    private Quaternion _initialRotation;
    private bool _isInitialized = false;

    public void Animate(Transform doorMesh, bool isOpen, float speed)
    {
        if(!_isInitialized)
        {
            InitializeRotation(doorMesh);
        }

        float finalAngle = reverse ? -openAngle : openAngle;
        float targetAngle = isOpen ? finalAngle : 0f;

        Quaternion targetRotation = _initialRotation * Quaternion.Euler(0f, targetAngle, 0f);

        doorMesh.localRotation = Quaternion.Slerp(doorMesh.localRotation, targetRotation, Time.deltaTime * speed);
    }

    private void InitializeRotation(Transform doorMesh)
    {
        _initialRotation = doorMesh.localRotation;
        _isInitialized = true;
    }

    public void ResetState()
    {
        _isInitialized = false;
    }
}
