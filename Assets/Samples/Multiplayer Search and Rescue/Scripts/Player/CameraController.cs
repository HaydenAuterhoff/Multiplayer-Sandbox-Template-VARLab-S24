using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VARLab.Sandbox.SAR
{
    public class CameraController : NetworkBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] protected CinemachineVirtualCamera virtualCamera;
        [SerializeField] protected int priority = 105;

        [Header("Input Settings")]
        [SerializeField] protected PlayerInput input;
        [SerializeField] protected float sensitivity = 12f;
        [SerializeField] private bool enableMouseMovement = false;

        //Private Fields
        private float xRotation = 0f;
        private InputAction lookAction;
        private InputAction clickAction;


        private void Start()
        {
            if (!virtualCamera)
            {
                virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            }

            if (!input)
            {
                input = GetComponent<PlayerInput>();
            }
        }


        public override void OnNetworkSpawn()
        {
            Debug.Log("Player camera controller networkspawned");

            //If we dont own this object, return
            if (!IsOwner) 
            {
                enabled = false;
                Debug.Log($"Object {name} is not mine, disabling");
                return; 
            }

            Initialize();
        }

        public void Initialize()
        {
            virtualCamera.Priority = priority;
            lookAction = input.actions.FindAction("Look");
            clickAction = input.actions.FindAction("Attack");
        }

        private void Update()
        {
            //If we dont own this object, return
            if (!IsSpawned || !IsOwner) { return; }

            if (!enableMouseMovement) { return; }

            //Only rotate camera if player is performing click action
            if (clickAction.ReadValue<float>() > 0.0f)
            {
                RotateCamera();
            }
        }

        private void RotateCamera()
        {
            // Get look value
            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            // Rotate Camera
            transform.Rotate(lookInput.x * sensitivity * Time.deltaTime * Vector3.up);

            // TODO have to manipulate '3rd-person follow' vcam height for vertical change
            //xRotation -= lookInput.y * sensitivity * Time.deltaTime;
            //xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}
