using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// This is a class that offers services relating to game codes
/// </summary>
public static class GameCodeServices
{
    //Protocol to use, set to wss for WebGL
    private const string transportProtocol = "wss";

    /// <summary>
    /// Attempts to join a relay service with a given join code
    /// </summary>
    /// <param name="joinCode">Join code to be used</param>
    /// <returns></returns>
    public static async Task JoinWithGameCode(string joinCode)
    {
        try
        {
            //Attempts to join the relay service
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            //Sets relay service data using the network managers unity transport
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, transportProtocol));
        }
        catch(Exception e)
        {
            //Logs the exception if something goes wrong
            Debug.LogException(e);
        }        
    }
}