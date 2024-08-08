using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class shows an example of playing an animation with client authority, meaning no calls
/// to the server necessary
/// </summary>
public class AnimationClientAuthority : NetworkBehaviour
{
    private Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        animator = GetComponent<Animator>();

        //Simple spawn system for two players, spwans them beside eachother for the purposes of this example
        transform.position = NetworkObjectId == 1 ? new Vector3(-0.5f, 0, 0) : new Vector3(3.5f, 0, 0);
    }

    /// <summary>
    /// As can be seen in the Update function when we press a button we do not need
    /// to call the server at all in order to play an animcation, we can directly call it as a client
    /// </summary>
    private void Update()
    {
        //If we are not the owner of this objet, or the animator is null, return
        if (!IsOwner || animator is null)
        {
            return;
        }

        //If we hit D open the door
        if (Input.GetKeyDown(KeyCode.D))
        {
            animator.SetTrigger("OpenDoors");
        }

        //If we hit S open the door
        if (Input.GetKeyDown(KeyCode.S))
        {
            animator.SetTrigger("CloseDoors");
        }
    }
}