using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public const float Gravity = -9.81f;

    //Serialized Variables
    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float runSpeed = 7;
    [SerializeField] private float gravityModifier = 3f;
    [SerializeField] private float fallingThreshold = -0.5f;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float jumpHeight = 1.5f;


    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    //Private Variables
    private Vector3 moveDirection;
    private Vector3 velocity;
    private Vector2 inputVector;
    private PlayerInput PlayerInput;
    
    private bool IsGrounded => Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);

    public override void OnNetworkSpawn()
    { 
      
        //If we dont own this object, return
        if (!IsOwner)
        {
            enabled = false;
            Debug.Log($"Object {name} is not mine, disabling");
            return;
        }

        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsSpawned || !IsOwner) { return; }

        ResetVelocityIfGrounded();
        HandleFalling();
        SetupNextPlayerMove();
        HandleGroundedActions();
        HandleInput();
        MovePlayer();
    }

    public void Initialize()
    {
        PlayerInput = GetComponent<PlayerInput>();
        PlayerInput.enabled = true;
        characterController.enabled = true;
    }

    public void OnMove(InputValue movementValue)
    {
        inputVector = movementValue.Get<Vector2>();
    }

    public void OnJump()
    {
        Jump();
    }

    #region Helper Methods

    private void ResetVelocityIfGrounded()
    {
        //If grounded, reset veloicty.y
        if (IsGrounded)
        {
            velocity.y = 0;
        }
    }
    private void HandleGroundedActions()
    {
        if (IsGrounded)
        {
            animator.SetTrigger("Landing");
        }
    }

    private void HandleInput()
    {
        if (moveDirection != Vector3.zero && !Input.GetKey(KeyCode.LeftShift))
        {
            Walk();

        }
        else if (moveDirection != Vector3.zero && Input.GetKey(KeyCode.LeftShift))
        {
            Run();
        }
        else if (moveDirection == Vector3.zero)
        {
            Idle();
        }

        moveDirection *= moveSpeed;

        if (Input.GetKeyDown(KeyCode.O))
        {
            Dance(true);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            Dance(false);
        }
    }
    private void SetupNextPlayerMove()
    {
        //Setup next player movement
        moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        moveDirection = transform.TransformDirection(moveDirection);
    }
    private void HandleFalling()
    {
        //If we are falling, play falling animation
        if (velocity.y < fallingThreshold)
        {
            Falling(true);
        }
        else
        {
            Falling(false);
        }
    }
    #endregion
    #region Animation Methods
    private void Falling(bool value)
    {
        animator.SetBool("Falling", value);
    }


    private void MovePlayer()
    {
        characterController.Move(moveDirection * Time.deltaTime);
        velocity.y += Gravity * gravityModifier * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    private void Idle()
    {
        animator.SetFloat("Speed", 0);
    }

    private void Walk()
    {
        moveSpeed = walkSpeed;
        animator.SetFloat("Speed", 0.5f);
    }

    private void Run()
    {
        moveSpeed = runSpeed;
        animator.SetFloat("Speed", 1);
    }

    private void Jump()
    {
        animator.SetTrigger("Jump");
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityModifier);
    }
    private void Dance(bool value)
    {
        animator.SetBool("IsDancing", value);
    }
    #endregion
}