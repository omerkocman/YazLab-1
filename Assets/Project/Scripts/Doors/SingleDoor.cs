using System;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DoorType
{
    Rotating,
    Sliding
}

[SelectionBase]
public class SingleDoor : DoorBase
{
    [Header("Settings")]
    public DoorType doorType = DoorType.Rotating;

    public RotatingDoorMovement rotatingStrategy = new RotatingDoorMovement();
    public SlidingDoorStrategy slidingStrategy = new SlidingDoorStrategy();

    private IDoorMovementStrategy _currentStrategy => doorType == DoorType.Rotating ? rotatingStrategy : slidingStrategy;

    private DoorType _lastDoorType;
    private Quaternion _defaultLocalRotation;
    private Vector3 _defaultLocalPosition;

    [SerializeField]
    private Collider _playerInside = null;

    void Start()
    {
        if(doorMesh != null)
        {
            SetDefaultTransformProperties();
        }

        _lastDoorType = doorType;
    }

    void Update()
    {
        if(doorType != _lastDoorType)
        {
            ResetMeshTransform();
            _lastDoorType = doorType;
        }

        if(_playerInside != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Inventory playerInventory = _playerInside.GetComponentInParent<Inventory>();
            print("interacted with door");
            Interact(playerInventory);
        }

        _currentStrategy?.Animate(doorMesh, isOpen, transitionSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _playerInside = other;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        _playerInside = null;
    }
    private void ResetMeshTransform()
    {
        if (doorMesh == null) return;
        
        isOpen = false;

        doorMesh.localRotation = _defaultLocalRotation;
        doorMesh.localPosition = _defaultLocalPosition;


        slidingStrategy.ResetState();
    }

    private void SetDefaultTransformProperties()
    {
        _defaultLocalRotation = doorMesh.localRotation;
        _defaultLocalPosition = doorMesh.localPosition;
    }
}
