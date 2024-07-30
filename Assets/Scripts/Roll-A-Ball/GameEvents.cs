using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Analytics;

namespace VARLab.Sandbox.Samples
{

    /// <summary>
    ///     Interface between <see cref="PlayFabClientAPI"/> 
    ///     and the Roll-a-Ball Game environment.
    /// </summary>
    public class GameEvents : MonoBehaviour
    {
        public const string EventNameScored = "player_scored";
        public const string EventKeyScored = "Score";
        public const string EventNameCompleted = "complete_event";
        public const string EventKeyCompleted = "completed_key";
        public const string EventValueCompleted = "game_completed";

        [Tooltip("The username used to login to the Analytics platform")]
        public string Username = "Sandbox";

        [Header("Analytics Event Callbacks")]
        public UnityEvent LoginCompleted;
        public UnityEvent<string> Response;

        /// <summary>
        ///     Generates a timestamp in the form HH:MM:SS for the time 
        ///     since the application launched.
        /// </summary>
        /// <returns>Timestamp as string</returns>
        public static string Timestamp()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        ///     Generates a <see cref="LoginWithCustomIDRequest"/> using the stored username
        ///     and sends it to the PlayFab endpoint.
        /// </summary>
        /// <remarks>
        ///     A successful response is received by the function
        ///     <see cref="LoginResultHandler(LoginResult)"/>
        ///     while an error response is received by the function
        ///     <see cref="GenericErrorHandler(PlayFabError)"/>
        /// </remarks>
        public void SendLoginEvent()
        {
            CoreAnalytics.Initialize();
            CoreAnalytics.LoginUser(Username, LoginResultHandler, GenericErrorHandler);
        }

        /// <summary>
        ///     Uses the custom <see cref="ScoreEventRequest"/> to send an event 
        ///     to the PlayFab endpoint containing the player's current score.
        /// </summary>
        /// <remarks>
        ///     This function is subscribed to the <see cref="GameManager.Scored"/> event
        ///     in the scene, so that it can fire every time the player's score changes.
        /// </remarks>
        /// <param name="input">
        ///     The player's current score, as an integer
        /// </param>
        public void SendScoreEvent(int value)
        {
            CoreAnalytics.CustomEvent(
                EventNameScored,
                EventKeyScored,
                value,
                ClientEventResponseHandler,
                GenericErrorHandler);
        }

        /// <summary>
        ///     Uses the custom <see cref="GameCompleteEventRequest"/> to send an event
        ///     to the PlayFab endpoint indicating that the game is complete.
        /// </summary>
        /// <remarks>
        ///     This function is subscribed to the <see cref="GameManager.Completed"/> event
        ///     in the scene, so that it can fire when the win condition has been met.
        /// </remarks>
        public void SendCompleteEvent()
        {
            CoreAnalytics.CustomEvent(
                EventNameCompleted,
                EventKeyCompleted,
                EventValueCompleted,
                ClientEventResponseHandler,
                GenericErrorHandler);
        }

        /// <summary>
        ///     Callback used for the login event in order to capture the response from PlayFab.
        /// </summary>
        /// <remarks>
        ///     In addition to generating a response string to pass to the <see cref="Response"/>
        ///     event, this function also invokes the <see cref="LoginCompleted"/> event
        ///     so that other objects (such as the GameManager) can use a successful login event
        ///     to start the game. 
        ///     
        ///     The function <see cref="GameManager.StartGame"/> is subscribed in the scene to
        ///     the LoginCompleted event.
        /// </remarks>
        /// <param name="result">Response from the PlayFab service</param>
        public void LoginResultHandler(LoginResult result)
        {
            Response?.Invoke($"User {Username} ({result.PlayFabId}) logged in at {Timestamp()}");
            LoginCompleted?.Invoke();
        }

        /// <summary>
        ///     Callback used for all PlayFab events which provide 
        ///     a <see cref="WriteEventResponse"/> back to the client.
        /// </summary>
        /// <remarks>
        ///     The response typically includes the initial request body, which is used
        ///     to log the event name and its time of completion.
        /// </remarks>
        /// <param name="response">Response from the PlayFab service</param>
        public void ClientEventResponseHandler(WriteEventResponse response)
        {
            if (response.Request is WriteClientPlayerEventRequest request)
            {
                Response?.Invoke($"{request.EventName} sent successfully at {Timestamp()}");
            }

        }

        /// <summary>
        ///     Error handler callback for all PlayFab events. The response is simply
        ///     passed to the <see cref="Response"/> event, which can be used to log the error
        ///     or handle it elsewhere.
        /// </summary>
        /// <param name="error">Response payload containing the error that occurred.</param>
        public void GenericErrorHandler(PlayFabError error)
        {
            Response?.Invoke(error.ErrorMessage);
        }
    }
}