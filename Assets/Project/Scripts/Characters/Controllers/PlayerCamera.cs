using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _playerRb;
    [SerializeField] private Transform _cameraHolder;

    [Header("Sensitivity & Limits")]
    [SerializeField] private float mouseSensitivity = 0.15f; // Rahat dönüş için 1 - 2 arası ideal
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float smoothTime = 0.02f;

    private float _pitch = 0f;
    private float _yaw = 0f;

    private Vector2 _currentMouseDelta;
    private Vector2 _currentMouseDeltaVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_playerRb != null)
        {
            _yaw = _playerRb.rotation.eulerAngles.y;
        }
    }


    void LateUpdate()
    {
        if (Mouse.current == null) return;

        Vector2 targetMouseDelta = Mouse.current.delta.ReadValue();

        // Fare zıplamalarını yumuşat
        _currentMouseDelta = Vector2.SmoothDamp(
            _currentMouseDelta,
            targetMouseDelta,
            ref _currentMouseDeltaVelocity,
            smoothTime
        );

        float mouseX = _currentMouseDelta.x * mouseSensitivity;
        float mouseY = _currentMouseDelta.y * mouseSensitivity;

        // 1. Dikey Bakış (Kafa)
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        _cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

        // 2. Yatay Bakış (Gövde)
        _yaw += mouseX;
        if (_playerRb != null)
        {
            // Doğrudan rotation ataması Constraints ile çakışmaz ve titreme yapmaz
            _playerRb.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }
    }
}