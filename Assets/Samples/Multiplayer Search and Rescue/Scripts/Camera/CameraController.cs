using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VARLab.Sandbox.SAR
{
    public class CameraController : NetworkBehaviour
    {
        //Serialized Fields
        [SerializeField] private float mouseSensitivity = 5f;
        [SerializeField] private PlayerInput input;

        //true = enabled, false = disabled
        [SerializeField] private bool enableMouseMovement = false;
        [SerializeField] private KeyCode changeMouseMovementKey;

        //Private Fields
        private Transform player;
        private float xRotation = 0f;
        private InputAction lookAction;
        private InputAction clickAction;

        private void Start()
        {
            //If we dont own this object, return
            if (!IsOwner) { return; }

            GetComponent<CinemachineVirtualCamera>().Priority = 100;
            lookAction = input.actions.FindAction("Look");
            clickAction = input.actions.FindAction("Attack");
            player = transform.parent;
        }

        private void Update()
        {
            //If we dont own this object, return
            if (!IsOwner) { return; }

            //Only rotate camera if player is performing click action
            if (clickAction.ReadValue<float>() > 0.0f && enableMouseMovement)
            {
                RotateCamera();
            }
        }

        private void RotateCamera()
        {
            // Get look value
            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            // Rotate Camera
            player.Rotate(lookInput.x * mouseSensitivity * Time.deltaTime * Vector3.up);
            xRotation -= lookInput.y * mouseSensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}
