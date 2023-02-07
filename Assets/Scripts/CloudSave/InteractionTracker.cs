using Newtonsoft.Json;
using UnityEngine;
using VARLab.CloudSave;
using VARLab.Interactions;


/// <summary>
///     Tracks the number of 'Click' interactions on an <see cref="Interactable"/> object.
/// </summary>
/// <remarks>
///     This component is intended as an example of the <see cref="VARLab.CloudSave"/> system.
///     The "state" of the object can be saved to persistent storage and then reloaded on startup.
/// </remarks>
[CloudSaved]
[JsonObject(MemberSerialization.OptIn)]
[RequireComponent(typeof(Interactable))]
public class InteractionTracker : MonoBehaviour, ICloudSerialized, ICloudDeserialized
{

    [JsonProperty]
    protected int interactionCount = 0;

    /// <summary>
    ///     Tracks the number of "Click" interactions
    /// </summary>
    public int InteractionCount => interactionCount;


    public void LogInteractionCount()
    {
        Debug.Log($"{name} has been interacted with {interactionCount} times");
    }

    public void HandleClickInteraction()
    {
        ++interactionCount;
        //LogInteractionCount();
    }

    public void OnSerialize()
    {
        Debug.Log($"Saving state of '{name}' with {interactionCount} interactions");
    }

    public void OnDeserialize()
    {
        Debug.Log($"Loaded state of '{name}' with {interactionCount} interactions");
    }
}
