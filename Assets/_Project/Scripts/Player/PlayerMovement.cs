using System;
using UnityEngine;


[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;


    private PlayerInputHandler input;
    private CharacterController controller;

    private float _verticalVelocity;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = input.Move;

        float moveSpeed = CalculateMoveSpeed();
        
        Vector3 horizontalDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        Vector3 horizontalVelocity = horizontalDirection * moveSpeed;

        Vector3 verticalVelocity = CalculateGravity();

        Vector3 finalVelocity = horizontalVelocity + verticalVelocity;


        controller.Move(finalVelocity * Time.deltaTime);

    }



    private float CalculateMoveSpeed()
    {
        bool isSprinting = input.Sprint;
        return isSprinting ? sprintSpeed : walkSpeed;
    }

    private Vector3 CalculateGravity()
    {

        if(controller.isGrounded)
        {
            _verticalVelocity = groundedGravity;
        }

        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
        
        return transform.up * _verticalVelocity;
    }
}
