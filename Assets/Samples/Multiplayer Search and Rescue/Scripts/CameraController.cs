using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private PlayerInput input;

    private Transform player;

    private float xRotation = 0f;

    InputAction lookAction;
    InputAction clickAction;

    private void Start()
    {      
        lookAction = input.actions.FindAction("Look");
        clickAction = input.actions.FindAction("Attack");
        player = transform.parent;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
       if (clickAction.ReadValue<float>() > 0.0f)
       {
            RotateCamera();
       }
    }

    private void RotateCamera()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        player.Rotate(Vector3.up * lookInput.x * mouseSensitivity * Time.deltaTime);

        xRotation -= lookInput.y * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}