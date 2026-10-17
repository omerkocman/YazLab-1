
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;


    private CharacterController _controller;
    private PlayerInputHandler _input;
    private float sensivity = 0.1f;
    private float pitch;
    private float maxPitch = 85f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputHandler>();
        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 moveInput = _input.Move;

        float moveSpeed = CalculateMoveSpeed();

        Vector3 horizontalVelocity = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized * moveSpeed;
        Vector3 verticalVelocity = CalculateGravity();

        Vector3 velocity = (horizontalVelocity + verticalVelocity);



        //print("Horizontal Velocity" + horizontalVelocity.magnitude);
        //print("Vertical Velocity" + verticalVelocity.magnitude);


        _controller.Move(velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        HandleLook();
    }

    private void HandleLook()
    {
        Vector2 lookInput = _input.Look;

        transform.Rotate(Vector3.up * lookInput.x * sensivity);

        pitch -= lookInput.y * sensivity;

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        Camera.main.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private Vector3 CalculateGravity()
    {
        float targetGravity = _controller.isGrounded ? groundedGravity : gravity;
        
        return transform.up * targetGravity;
    }

    private float CalculateMoveSpeed()
    {
        bool isSprinting = _input.Sprint;

        return isSprinting ? sprintSpeed : walkSpeed;
    }
}
