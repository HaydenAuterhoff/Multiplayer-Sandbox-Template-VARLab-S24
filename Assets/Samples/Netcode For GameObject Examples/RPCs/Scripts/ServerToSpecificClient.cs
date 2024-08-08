using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This clsas is an example of a server sending something to a specific client. 
/// </summary>
public class ServerToSpecificClient : NetworkBehaviour
{
    void Update()
    {
        //If we are not the owner of this object, return
        if (!IsOwner)
        {
            return;
        }

        //If we hit S, ask the server for a random number
        if (Input.GetKeyDown(KeyCode.S))
        {
            RequestRandomNumberServerRpc(NetworkManager.Singleton.LocalClientId, new RpcParams());
        }
    }

    /// <summary>
    /// This function is called on the server, generates a random number and sends it back to the client
    /// </summary>
    /// <param name="playerId">The id of the player making the request</param>
    /// <param name="rpcParams">The RPC Params so that the server can send back to the correct client</param>
    [Rpc(SendTo.Server)]
    private void RequestRandomNumberServerRpc(ulong playerId, RpcParams rpcParams)
    {
        //If we are not the server, return
        if (!IsServer)
        {
            return;
        }

        //Log the player who requeted this action
        Debug.Log($"This was called by client with id: {playerId}. Now sending random number back to player");

        //Generate random number
        int numberToSend = Random.Range(1, 11);

        //Send it back to the client who requested the number
        SendBackToClientRpc(numberToSend, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    /// <summary>
    /// This function is called on a singular client by the server
    /// </summary>
    /// <param name="number"></param>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.SpecifiedInParams)]
    private void SendBackToClientRpc(int generatedNumber, RpcParams rpcParams)
    {
        //Logs the generated number
        Debug.Log($"Server sent back: {generatedNumber}");
    }
}