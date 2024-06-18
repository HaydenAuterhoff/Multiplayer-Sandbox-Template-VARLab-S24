using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace VARLab.Sandbox.Samples
{

    /// <summary>
    ///     The standard Player controller for Roll-A-Ball. Handles input events from the
    ///     <see cref="PlayerInput"/> class attached to this same GameObject using Unity's 
    ///     Send Message callback system.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Speed at which the ball (player) moves")]
        public float Speed = 10f;

        [Tooltip("Event invoked whenever the player comes in contact with a valid Pickup")]
        public UnityEvent Pickup;

        /// <summary>
        ///     Buffer for x-direction movement
        /// </summary>
        private float movementX;

        /// <summary>
        ///     Buffer for y-direction movement. 
        ///     This is represented in the game as z-direction movement, 
        ///     though it is received from the input system as the y value of a Vector2
        /// </summary>
        private float movementY;

        /// <summary>
        ///     The physics system body attached to this GameObject
        /// </summary>
        private Rigidbody body;


        public virtual void Start()
        {
            body = GetComponent<Rigidbody>();
        }

        /// <summary>
        ///     Movement is handled in FixedUpdate so that it is consistent with the physics
        ///     properties of objects in the scene
        /// </summary>
        public virtual void FixedUpdate()
        {
            Vector3 movement = new(movementX, 0, movementY);
            body.AddForce(movement * Speed);
        }


        /// <summary>
        ///     Input System callback providing a movement vector from user input
        /// </summary>
        /// <param name="input">
        ///     An InputValue that can be read as a <see cref="Vector2"/>
        /// </param>
        public virtual void OnMove(InputValue input)
        {
            Vector2 movementVector = input.Get<Vector2>();

            movementX = movementVector.x;
            movementY = movementVector.y;
        }

        /// <summary>
        ///     Unity standard callback for this object coming in contact with an
        ///     object with a collider. If the other object has a <see cref="Samples.Pickup"/>
        ///     component, then collision with that pickup is handled by "Collecting" the
        ///     pickup and invoking the Pickup event
        /// </summary>
        /// <param name="other">
        ///     An object which is expected to have a <see cref="Samples.Pickup"/> component.
        ///     If this component is missing, nothing happens.
        /// </param>
        public virtual void OnTriggerEnter(Collider other)
        {
            Pickup pickup = other.GetComponent<Pickup>();

            if (pickup)
            {
                pickup.Collect();
                Pickup?.Invoke();
            }
        }
    }
}