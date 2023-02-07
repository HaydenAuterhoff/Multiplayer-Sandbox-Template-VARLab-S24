using Cinemachine;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using VARLab.CloudSave;
using VARLab.CORECinema;


/// <summary>
///     Tracks the <see cref="CinemachineVirtualCamera"/> between sessions. The current camera and 
/// </summary>
/// <remarks>
///     This component is intended as an example of the <see cref="VARLab.CloudSave"/> system.
///     The state of the camera can be saved to persistent storage and then reloaded on startup.
/// </remarks>
[CloudSaved]
[JsonObject(MemberSerialization.OptOut)]
[RequireComponent(typeof(CinemachineBrain))]
public class ActiveCameraTracker : MonoBehaviour, ICloudSerialized, ICloudDeserialized
{

    [JsonProperty]
    protected string activeCameraName;

    [JsonProperty]
    protected float pathPosition = 0;

    [JsonProperty]
    protected float recomposerTilt = 0;

    [JsonProperty]
    protected float recomposerPan = 0;

    [JsonProperty]
    protected float recomposerZoom = 1;


    /// <summary>
    ///     Reads appropriate camera values from the <see cref="CinemachineBrain"/>
    ///     and its related components
    /// </summary>
    public void OnSerialize()
    {
        var brain = GetComponent<CinemachineBrain>();
        activeCameraName = brain.ActiveVirtualCamera.Name;

        recomposerPan = 0;
        recomposerTilt = 0;
        recomposerZoom = 1;

        var recomposer = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineRecomposer>();
        if (recomposer)
        {
            recomposerPan = recomposer.m_Pan;
            recomposerTilt = recomposer.m_Tilt;
            recomposerZoom = recomposer.m_ZoomScale;
        }

        var dolly = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponentInChildren<CinemachineTrackedDolly>();
        pathPosition = dolly ? dolly.m_PathPosition : 0;
    }

    public void OnDeserialize()
    {
        Debug.Log($"Loading settings for camera '{activeCameraName}'\n" +
            $"Zoom: {recomposerZoom}, Panning: ({recomposerTilt}, {recomposerPan}), Path: {pathPosition}");

        StartCoroutine(ApplyCameraSettingsCoroutine());
    }

    private IEnumerator ApplyCameraSettingsCoroutine()
    {
        var brain = GetComponent<CinemachineBrain>();

        if (!brain) { yield break; }

        CinemachineBlendDefinition blendCache = brain.m_DefaultBlend;
        brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);

        // Wait single frame to ensure blending mode is updated
        yield return new WaitForEndOfFrame();

        brain.GetComponent<CinemachineCameraPriorityManager>().ActivateCamera(activeCameraName);
        
        // Wait single frame to ensure camera priority has changed
        yield return new WaitForEndOfFrame();


        // Potentially would want to expose mutators through CinemachinePan, CinemachineZoom, and CinemachinePathMovement
        // to handle these settings
        var recomposer = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineRecomposer>();
        if (recomposer)
        {
            recomposer.m_Pan = recomposerPan;
            recomposer.m_Tilt = recomposerTilt;
        }

        // This will currently throw an exception on virtual cameras that do not expose zoom functionality
        try
        {
            var zoom = brain.GetComponent<CinemachineZoom>();
            if (zoom) { zoom.SetZoomScale(recomposerZoom); }
        }
        catch (NullReferenceException)
        {
            Debug.LogWarning("Attempting to load a camera that does not support zoom functionality.");
        }

        // This is setting path position directly as opposed to looking at CinemachinePathMovement
        var dolly = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponentInChildren<CinemachineTrackedDolly>();
        if (dolly) { dolly.m_PathPosition = pathPosition; }

        // Wait single frame to ensure no movement is attempted
        yield return new WaitForEndOfFrame();

        // Reset the camera blend properties
        brain.m_DefaultBlend = blendCache;

        yield return null;
    }
}
