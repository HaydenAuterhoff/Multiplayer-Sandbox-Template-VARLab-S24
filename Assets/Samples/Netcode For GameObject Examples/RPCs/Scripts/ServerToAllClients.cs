using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class is an example of the server executing a method on every client in the game
/// </summary>
public class ServerToAllClients : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.S) && IsServer)
        {
            InvokeOnAllClientsRpc();
        }
    }


    /// <summary>
    /// This is executed on every client (including the host)
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    //[Rpc(SendTo.Everyone)]
    private void InvokeOnAllClientsRpc()
    {
        Debug.Log("This is being called everywhere, invoked by server");
    }
}
