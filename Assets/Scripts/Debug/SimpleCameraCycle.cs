using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This is a sample script used for a build test 
///     of the Search and Rescue proof-of-concept project.
///     Allows a user to cycle through the cameras using the 
///     left and right arrows
/// </summary>
public class SimpleCameraCycle : MonoBehaviour
{
    public const int PriorityHigh = 100;
    public const int PriorityLow = 10;

    [Header("Runtime")]
    public bool ManualCycle = true;
    public bool AutoCycle = true;
    [Tooltip("Time in seconds between automatic camera cycles")]
    public float CycleDelay = 7f;

    [Header("Setup")]
    [Tooltip("List of virtual cameras in the scene")]
    public List<CinemachineVirtualCamera> Cameras;


    private int index = 0;
    private float cycleElapsed = 0f;
    private CinemachineVirtualCamera current;

    // Properties
    public CinemachineVirtualCamera Current => current;


    /// <summary>
    ///     Get reference to initial 'current' camera on Start
    /// </summary>
    void Start()
    {
        if (Cameras.Count == 0)
        {
            Debug.LogWarning("No virtual cameras to control");            
        }

        foreach (var cam in Cameras)
        {
            cam.Priority = PriorityLow;
        }

        current = Cameras[index];
        current.Priority = PriorityHigh;
    }

    /// <summary>
    ///     If the left or right arrow key is pressed, move to the previous 
    ///     or next camera, respectively
    /// </summary>
    void Update()
    {
        if (Cameras.Count == 0) { return; }

        if (CheckInput())
        {
            cycleElapsed = 0f;
            return;
        }

        if (AutoCycle)
        {
            cycleElapsed += Time.deltaTime;
            if (cycleElapsed >= CycleDelay)
            {
                Cycle();
                cycleElapsed = 0f;
            }
        }
    }

    bool CheckInput()
    {
        // Need to add count so that the result after mod will always be non-negative
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Cycle(forward: false);
            return true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Cycle(forward: true);
            return true;
        }

        return false;
    }

    private void Cycle(bool forward = true)
    {
        int count = Cameras.Count;
        index = (index + (forward ? 1 : -1) + count) % count;
        Switch(Cameras[index]);
    }

    /// <summary>
    ///     Switches the given <paramref name="target"/> to be the current
    ///     virtual camera
    /// </summary>
    private void Switch(CinemachineVirtualCamera target)
    {
        current.Priority = PriorityLow;

        current = target;

        current.Priority = PriorityHigh;
    }
}
