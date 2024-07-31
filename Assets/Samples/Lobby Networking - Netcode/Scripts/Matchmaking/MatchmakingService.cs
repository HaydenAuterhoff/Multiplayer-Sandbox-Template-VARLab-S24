using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using VARLab.Multiplayer.Lobbies;

public class MatchmakingService : MonoBehaviour
{
    private const int HeartbeatInterval = 15;
    private const int LobbyRefreshRate = 2;

    private const string RelayCode = "relayCode";
    private const string GameStatus = "gameStatus";
    private const string Protocol = "wss";
    private const string Lobby = "Lobby";

    public static MatchmakingService Instance { get; private set; }
    public static Lobby CurrentLobby { get; private set; }

    private static UnityTransport Transport;

    private static CancellationTokenSource heartbeatSource;
    private static CancellationTokenSource updateLobbySource;


    public event Action<Lobby> CurrentLobbyRefreshed;

    public string JoinCode => CurrentLobby.Data[RelayCode].Value;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        Transport = FindFirstObjectByType<UnityTransport>();
    }

    // TODO replace this with Reset(), review static members
    public void ResetStatics()
    {
        if (Transport != null)
        {
            Transport.Shutdown();
            Transport = null;
        }
        CurrentLobby = null;
    }

    // Lobby query functions

    public async UniTask<List<Lobby>> GatherLobbies()
    {
        var options = new QueryLobbiesOptions
        {
            Count = 15,

            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ)
            }
        };
        var allLobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
        return allLobbies.Results;
    }

    public async UniTask CreateLobbyWithAllocation(LobbyData data)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(data.Capacity);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            data.JoinCode = joinCode;

            //string playerName = PlayerPrefs.GetString(PlayerNameInput.DisplayName);
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { RelayCode, new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                    { GameStatus, new DataObject(DataObject.VisibilityOptions.Public, Lobby) }
                }
            };
            CurrentLobby = await Lobbies.Instance.CreateLobbyAsync(data.Name, data.Capacity, options);
            Transport.SetRelayServerData(new RelayServerData(allocation, Protocol));

            StartConnectionHeartbeat();
            StartBackgroundLobbyRefresh();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async UniTask JoinLobbyWithAllocation(string lobbyId)
    {
        CurrentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);

        var allocation = await RelayService.Instance.JoinAllocationAsync(CurrentLobby.Data[RelayCode].Value);
        Transport.SetRelayServerData(new RelayServerData(allocation, Protocol));
        CurrentLobby.Data.TryGetValue(GameStatus, out DataObject dataObject);

        if (dataObject.Value == Lobby)
        {
            StartBackgroundLobbyRefresh();
        }
    }

    /// <summary>
    /// Attempts to join a relay service with a given join code
    /// </summary>
    /// <param name="joinCode">Join code to be used</param>
    /// <returns></returns>
    public async UniTask JoinWithGameCode(string joinCode)
    {
        try
        {
            //Attempts to join the relay service
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            //Sets relay service data using the network managers unity transport
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, Protocol));
        }
        catch (Exception e)
        {
            //Logs the exception if something goes wrong
            Debug.LogException(e);
        }
    }

    public async UniTask<string> JoinLobbyAndAllocationByGameCode(string gameCode)
    {
        string targetLobbyStatus = string.Empty;
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions());

        Lobby targetLobby = queryResponse.Results.FirstOrDefault(lobby =>
        lobby.Data.TryGetValue(RelayCode, out DataObject dataObject) && dataObject.Value == gameCode);

        if (targetLobby != null) { await JoinLobbyWithAllocation(targetLobby.Id); }

        if (targetLobby.Data.TryGetValue(GameStatus, out DataObject dataObject)) { targetLobbyStatus = dataObject.Value; }

        return targetLobbyStatus;
    }

    public async UniTask<bool> LockLobby()
    {
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions { IsLocked = true });
            return true;
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to lock lobby: {e}");
            return false;
        }
    }

    public async UniTask LeaveLobby()
    {
        StopConnectionHeartbeat();
        StopBackgroundLobbyRefresh();

        if (CurrentLobby == null)
        {
            Debug.LogError("No lobby to leave");
            CurrentLobbyRefreshed?.Invoke(CurrentLobby);
            return;
        }

        try
        {
            if (CurrentLobby.HostId == AuthenticationManager.PlayerId)
            {
                Debug.Log("Closing lobby as host");
                await Lobbies.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                Debug.Log("Lobby closed");
            }
            else
            {
                Debug.Log("Leaving lobby as player");
                await Lobbies.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationManager.PlayerId);
                Debug.Log("Lobby left");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            // TODO pass error data outside 
        }
        finally
        {
            Debug.Log("Current Lobby is now null");
            CurrentLobby = null;
            CurrentLobbyRefreshed?.Invoke(CurrentLobby);
        }

    }

    private async void StartConnectionHeartbeat()
    {
        heartbeatSource = new CancellationTokenSource();
        while (!heartbeatSource.IsCancellationRequested && CurrentLobby != null)
        {
            await Lobbies.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            await UniTask.Delay(HeartbeatInterval * 1000);
        }
    }

    private void StopConnectionHeartbeat() => heartbeatSource.Cancel();

    private async void StartBackgroundLobbyRefresh()
    {
        updateLobbySource = new CancellationTokenSource();

        await UniTask.Delay(LobbyRefreshRate * 1000);
        while (!updateLobbySource.IsCancellationRequested && CurrentLobby != null)
        {
            CurrentLobby = await Lobbies.Instance.GetLobbyAsync(CurrentLobby.Id);
            CurrentLobbyRefreshed?.Invoke(CurrentLobby);
            await UniTask.Delay(LobbyRefreshRate * 1000);
        }
    }

    private void StopBackgroundLobbyRefresh() => updateLobbySource?.Cancel();

}