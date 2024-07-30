using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace VARLab.Sandbox.Samples
{

    /// <summary>
    ///     A controller for the Roll-a-Ball sample scene.
    /// </summary>
    /// <remarks>
    ///     Handles the player's score and the game "win" condition.
    /// </remarks>
    public class GameManager : MonoBehaviour
    {
        [Tooltip("The player's current score")]
        public int Score = 0;

        [Tooltip("The target score for the game to be \"won\". " +
            "If set to -1, the target score will be equal to the number of Pickups in the scene")]
        public int TargetScore = -1;


        [Tooltip("Indicates whether the Started event should be on launch or wait " +
            "until it is called externally.")]
        public bool StartAutomatically = false;

        // Events which are exposed to the inspector and can be serialized to respond 
        // to various game state changes.
        [Header("Game State Events")]

        [Tooltip("Invoked in the Awake method of this object")]
        public UnityEvent Initialized;

        [Tooltip("Invoked when the game 'starts', to set game objects into their starting states. " +
            "Analytics Login event is serialized here so that it can be initiated externally. " +
            "Will be called internally only if StartAutomatically is set to true")]
        public UnityEvent Started;

        [Tooltip("Invoked whenever the player's score changes.")]
        public UnityEvent<int> Scored;
        public UnityEvent Completed;


        /// <summary>
        ///     On awake, the Initialized event is invoked to synchronize the Game Manager with 
        ///     other objects that may want to be listening to its events.
        /// </summary>
        public void Awake()
        {
            InitializeGame();
        }

        /// <summary>
        ///     Initializes the TargetScore for the game if it is not already set.
        /// </summary>
        /// <remarks>
        ///     The game can be 'automatically' started if the <see cref="StartAutomatically"/>
        ///     flag is set to true. Otherwise, the game must wait for an external class
        ///     to call this method to formally start the game.
        /// </remarks>
        public void Start()
        {
            if (TargetScore < 0)
            {
                TargetScore = FindObjectsOfType<Pickup>().Length;
                Debug.Log($"Target score is {TargetScore}");
            }

            if (StartAutomatically)
            {
                StartGame();
            }
        }


        /// <summary>
        ///     Each update, the game is checking for the win condition. If the condition
        ///     is satisfied, the Completed event is invoked to let listeners know that
        ///     the game has been won.
        /// </summary>
        public void Update()
        {
            if (enabled && Score >= TargetScore)
            {
                CompleteGame();
                enabled = false;
            }
        }

        /// <summary>
        ///     Invokes the Initialized event, only if this object is enabled
        /// </summary>
        public void InitializeGame()
        {
            if (!enabled) { return; }
            
            Initialized?.Invoke();
        }

        /// <summary>
        ///     Invokes the <see cref="Started"/> event, letting listeners know that 
        ///     the game has started.
        /// </summary>
        /// <remarks>
        ///     With <see cref="StartAutomatically"/> set to false, we will need to wait
        ///     until another object (the <see cref="GameEvents"/> analytics events manager)
        ///     calls this StartGame method in order to start the game.
        ///     
        ///     Ensure that this method is a listener for the 
        ///     <see cref="GameEvents.LoginCompleted"/> event.
        /// </remarks>
        public void StartGame() 
        {
            if (!enabled) { return; }

            Started?.Invoke();
        }

        /// <summary>
        ///     Uses the SceneManager to reload this scene, causing the game to restart
        /// </summary>
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        ///     Invokes the <see cref="Completed"/> event, indicating to listeners that 
        ///     the game's win condition has been met.
        /// </summary>
        /// <remarks>
        ///     The <see cref="GameEvents.SendCompleteEvent"/> function is serialized as
        ///     a listener to the <see cref="Completed"/> event, so that the corresponding
        ///     analytics event can be sent on game end.
        /// </remarks>
        public void CompleteGame()
        {
            if (!enabled) { return; }

            Completed?.Invoke();
        }

        /// <summary>
        ///     Increments the player's score and invokes the <see cref="Scored"/> event.
        /// </summary>
        /// <remarks>
        ///     This function is serialized as a listener to the <see cref="PlayerController.Pickup"/> 
        ///     event.
        ///     Additionally, the <see cref="GameEvents.SendScoreEvent(int)"/> function is subscribed 
        ///     to the <see cref="Scored"/> event so that the corresponding analytics event
        ///     can be sent any time the player's score changes.
        /// </remarks>
        public void PlayerPickup()
        {
            if (!enabled) { return; }

            Score++;

            Scored?.Invoke(Score);
        }
    }
}