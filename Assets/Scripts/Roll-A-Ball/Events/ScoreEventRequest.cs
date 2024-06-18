using PlayFab.ClientModels;

namespace VARLab.Sandbox.Samples
{

    /// <summary>
    ///     Custom event request type which extends <see cref="WriteClientPlayerEventRequest"/>
    ///     in order to provide a pre-determined event name of "player_scored" and receive
    ///     a parameter for the player's score.
    /// </summary>
    /// <remarks>
    ///     As the event body takes values of type <see langword="object"/>, it does not matter
    ///     what data type is provided in the constructor, as long as it can be string-ified to
    ///     be written as JSON. 
    ///     
    ///     The <see cref="GameEvents"/> manager will be providing integer values for the 
    ///     player's score in this case.
    /// </remarks>
    public class ScoreEventRequest : WriteClientPlayerEventRequest
    {
        public ScoreEventRequest(object score = null)
        {
            score ??= 0;

            EventName = "player_scored";
            Body = new()
            {
                { "Score", score }
            };
        }
    }
}