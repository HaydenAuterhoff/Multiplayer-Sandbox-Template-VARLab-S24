using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class shows an example of a Server Authorative based movement system, as can be seen the actual movement is done on
/// the server, the client has to request the server to move for it
/// </summary>
public class ServerAuthorityMovement : NetworkBehaviour
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

        //Because the server is moving the player the character controller needs to be active on the server
        characterController = GetComponent<CharacterController>();
        characterController.enabled = IsServer;
    }

    void Update()
    {
        if (!IsOwner) { return; }
        GetPlayerInput();
    }

    /// <summary>
    /// Handles moving the player by taking what was recieved from OnMove and moving the player via the Character Controller
    /// </summary>
    private void GetPlayerInput()
    {
        moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        moveDirection = transform.TransformDirection(moveDirection);

        if (moveDirection != Vector3.zero && !Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = walkSpeed;
        }
        moveDirection *= moveSpeed;

        //If we are the server we can directly call the Move function, otherwise we need to request the server
        //to validate our movement
        if (IsServer && IsLocalPlayer && moveDirection != Vector3.zero)
        {
            Move(moveDirection);
        }
        else if (IsClient && IsLocalPlayer && moveDirection != Vector3.zero)
        {
            MovePlayerRpc(moveDirection);
        }
    }

    /// <summary>
    /// Where the server actually moves the player
    /// </summary>
    /// <param name="movement">The clients movement value</param>
    private void Move(Vector3 movement)
    {
        characterController.Move(movement * Time.deltaTime);
    }

    /// <summary>
    /// This is called on the server beacuse of the Rpc
    /// </summary>
    /// <param name="movement">The clients movement value</param>
    [Rpc(SendTo.Server)]
    private void MovePlayerRpc(Vector3 movement)
    {
        Move(movement);
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