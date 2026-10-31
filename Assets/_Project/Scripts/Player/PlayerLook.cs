using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerLook : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float sensitivity = 0.1f;

    private PlayerInputHandler input;
    private float pitch;
    private float maxPitch = 90f;


    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();

        if(cameraPivot == null)
        {
            Debug.LogWarning("[Player Look] cameraPivot is null!");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        Vector2 lookInput = input.Look;

        transform.Rotate(Vector3.up * lookInput.x * sensitivity);

        pitch -= lookInput.y * sensitivity;

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        cameraPivot.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
