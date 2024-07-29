using Unity.Netcode;
using UnityEngine;

public class DoorAnimationController : NetworkBehaviour
{
    //false = closed, true = open
    private NetworkVariable<bool> doorState =
        new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Animator doorAnimator;
    private int playerCount;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();
    }

    [Rpc(SendTo.Server)]
    public void OpenDoorRpc()
    {
        if (!IsServer) { return; }
        playerCount++;
        if (!doorState.Value)
        {
            doorState.Value = true;
            doorAnimator.SetTrigger("OpenDoors");
        }
    }

    [Rpc(SendTo.Server)]
    public void CloseDoorRpc()
    {
        if (!IsServer) { return; }
        playerCount--;
        if (doorState.Value && playerCount == 0)
        {
            doorState.Value = false;
            doorAnimator.SetTrigger("CloseDoors");
        }
    }
}