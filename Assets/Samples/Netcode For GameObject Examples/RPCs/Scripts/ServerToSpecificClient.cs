using Unity.Netcode;
using UnityEngine;

public class ServerToSpecificClient : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.S))
        {
            RequestRandomNumberServerRpc(NetworkManager.Singleton.LocalClientId, new RpcParams());
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestRandomNumberServerRpc(ulong playerId, RpcParams rpcParams)
    {
        Debug.Log($"This was called by client with id: {playerId}. Now sending random number back to player");
        SendBackToClientRpc(Random.Range(1, 11), RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendBackToClientRpc(int number, RpcParams rpcParams)
    {
        Debug.Log($"Server sent back: {number}");
    }
}
