using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class is an example of a client sending a request to the server to run a method
/// </summary>
public class ClientToServer : NetworkBehaviour
{
    void Update()
    {
        //If we are not the owner of this object, return
        if (!IsOwner)
        {
            return;
        }

        //If we hit S, request a method call on the server.
        if (Input.GetKeyDown(KeyCode.S))
        {
            InvokeOnServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    /// <summary>
    /// This method gets called on the server because of the RPC attribute
    /// </summary>
    /// <param name="playerId"></param>
    [Rpc(SendTo.Server)]
    private void InvokeOnServerRpc(ulong playerId)
    {
        //Only the server should be running this, so if we aren't the server, return
        if (!IsServer)
        {
            return;
        }

        Debug.Log($"This is being called on the server, invoked from client with id: {playerId}");
    }
}