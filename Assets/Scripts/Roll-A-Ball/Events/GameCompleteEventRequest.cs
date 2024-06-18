using PlayFab.ClientModels;

namespace VARLab.Sandbox.Samples
{

    /// <summary>
    ///     Custom event request type which extends <see cref="WriteClientPlayerEventRequest"/>
    ///     in order to provide a pre-determined event name of "game_completed"
    /// </summary>
    public class GameCompleteEventRequest : WriteClientPlayerEventRequest
    {
        public GameCompleteEventRequest()
        {
            EventName = "game_completed";
        }
    }
}