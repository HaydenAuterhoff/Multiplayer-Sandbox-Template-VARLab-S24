using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    //Serialized Variables
    [SerializeField] private float moveSpeed = 0, walkSpeed = 5, runSpeed = 7, gravity = -9.81f, fallingThreshold = -0.5f, groundCheckDistance = 0.5f, jumpHeight = 1.5f;
    [SerializeField] private LayerMask groundMask;

    //Private Variables
    private Vector3 moveDirection, velocity;
    private Vector2 inputVector;

    private CharacterController controller;
    private Animator animator;
    private bool isGrounded { get { return Physics.CheckSphere(transform.position, groundCheckDistance, groundMask); } }
    
    #endregion
    #region Unity Methods
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ResetVelocityIfGrounded();
        HandleFalling();
        SetupNextPlayerMove();
        HandleGroundedActions();
        MovePlayer();
    }

    private void OnMove(InputValue movementValue)
    {
        inputVector = movementValue.Get<Vector2>();
    }
    private void OnJump()
    {
        Jump();
    }
    #endregion
    #region Helper Methods
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

    private void ResetVelocityIfGrounded()
    {
        //If grounded, reset veloicty.y
        if (isGrounded)
        {
            velocity.y = 0;
        }
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

    private void SetupNextPlayerMove()
    {
        //Setup next player movement
        moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        moveDirection = transform.TransformDirection(moveDirection);
    }

    private void HandleGroundedActions()
    {
        if (isGrounded)
        {
            animator.SetTrigger("Landing");
            HandleInput();
        }
    }

    private void MovePlayer()
    {
        controller.Move(moveDirection * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    #endregion
    #region Setting Animations
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
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
    }

    private void Falling(bool value)
    {
        animator.SetBool("Falling", value);
    }

    private void Dance(bool value)
    {
        animator.SetBool("IsDancing", value);
    }
    #endregion
}