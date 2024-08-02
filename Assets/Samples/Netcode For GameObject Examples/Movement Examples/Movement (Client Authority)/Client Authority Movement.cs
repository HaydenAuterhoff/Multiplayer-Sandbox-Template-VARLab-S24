using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class shows an example of a Client Authorative based movement system, as can be seen there is no calls to a server
/// in this class, it is completley done on the client.
/// </summary>
public class ClientAuthorityMovement : NetworkBehaviour
{
    private Vector3 moveDirection;
    private Vector2 inputVector;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float walkSpeed = 5;

    private PlayerInput playerInput;

    private CharacterController characterController;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        transform.localPosition = new Vector3(0, 1, 0);
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = IsOwner;

        characterController = GetComponent<CharacterController>();
        characterController.enabled = IsOwner;
    }

    void Update()
    {
        if (!IsOwner) { return; }
        MovePlayer();
    }

    /// <summary>
    /// Handles moving the player by taking what was recieved from OnMove and moving the player via the Character Controller
    /// </summary>
    private void MovePlayer()
    {
        moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        moveDirection = transform.TransformDirection(moveDirection);
        if (moveDirection != Vector3.zero)
        {
            moveSpeed = walkSpeed;
        }
        moveDirection *= moveSpeed;

        //The client directly moves their own player
        characterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Listens for movement from the client
    /// </summary>
    /// <param name="movementValue">The movement value of the player</param>
    private void OnMove(InputValue movementValue)
    {
        //stores the players movement value
        inputVector = movementValue.Get<Vector2>();
    }
}