using Cinemachine;
using System.Collections;
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

    [Tooltip("List of virtual cameras in the scene")]
    public List<CinemachineVirtualCamera> Cameras;

    private int index = 0;
    private CinemachineVirtualCamera current;

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
        int count = Cameras.Count;

        if (count < 1) { return; }
        
        // Need to add count so that the result after mod will always be non-negative
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            index = (index - 1 + count) % count;
            Switch(Cameras[index]);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            index = (index + 1 + count) % count;
            Switch(Cameras[index]);
        }
    }

    /// <summary>
    ///     Switches the given <paramref name="target"/> to be the current
    ///     virtual camera
    /// </summary>
    public void Switch(CinemachineVirtualCamera target)
    {
        current.Priority = PriorityLow;

        current = target;

        current.Priority = PriorityHigh;
    }
}
