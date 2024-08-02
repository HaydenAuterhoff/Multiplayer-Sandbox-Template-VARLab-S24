using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class shows an example of playing an animation with server authority, meaning calls
/// to the server are necessary
/// </summary>
public class AnimationServerAuthority : NetworkBehaviour
{
    private Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        animator = GetComponent<Animator>();
        transform.position = NetworkObjectId == 1 ? new Vector3(-0.5f, 0, 0) : new Vector3(3.5f, 0, 0);
    }

    /// <summary>
    /// This update function calls each animation twice, once through the server and once without,
    /// since this example is server authorative you will notice that the door will not open on all clients if 
    /// you hit the key that does not go through the server (D and C) but it will play for everyone, all be it delayed if 
    /// you hit the other 2 (S and X).
    /// </summary>
    private void Update()
    {
        if (!IsOwner || animator is null) { return; }
        if (Input.GetKeyDown(KeyCode.D))
        {
            animator.SetTrigger("OpenDoors");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            PlayAnimationRpc("OpenDoors");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetTrigger("CloseDoors");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayAnimationRpc("CloseDoors");
        }
    }

    /// <summary>
    /// Plays the animation through the server
    /// </summary>
    /// <param name="animationTrigger">Name of trigger to invoke</param>
    [Rpc(SendTo.Server)]
    private void PlayAnimationRpc(string animationTrigger)
    {
        animator.SetTrigger(animationTrigger);
    }
}