using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MatchmakingService : MonoBehaviour
{
    private const int HeartbeatInterval = 15;
    private const int LobbyRefreshRate = 2;
    private const string relayCode = "relayCode", gameStatus = "gameStatus", protocol = "wss", Lobby = "Lobby";
    public static MatchmakingService Instance { get; private set; }

    private static UnityTransport Transport;
    private static Lobby _currentLobby;
    private static CancellationTokenSource _heartbeatSource, _updateLobbySource;

    //private static UnityTransport Transport
    //{
    //    get => _transport != null ? _transport : _transport = FindFirstObjectByType<UnityTransport>();
    //    set => _transport = value;
    //}
    private MatchmakingService() { }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        Transport = FindFirstObjectByType<UnityTransport>();
    }

    public event Action<Lobby> CurrentLobbyRefreshed;

    public void ResetStatics()
    {
        if (Transport != null)
        {
            Transport.Shutdown();
            Transport = null;
        }
        _currentLobby = null;
    }

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

    public async UniTask CreateLobbyWithAllocation(CreateLobbyScreen.LobbyData data)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(data.MaxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            data.JoinCode = joinCode;

            string playerName = PlayerPrefs.GetString(PlayerNameInput.DisplayName);
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { relayCode, new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                    { gameStatus, new DataObject(DataObject.VisibilityOptions.Public, Lobby) }
                }
            };
            _currentLobby = await Lobbies.Instance.CreateLobbyAsync(data.Name, data.MaxPlayers, options);
            Transport.SetRelayServerData(new RelayServerData(allocation, protocol));
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }

        Heartbeat();
        PeriodicallyRefreshLobby();
    }

    public async UniTask JoinLobbyWithAllocation(string lobbyId)
    {
        _currentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);
        
        var allocation = await RelayService.Instance.JoinAllocationAsync(_currentLobby.Data[relayCode].Value);
        Transport.SetRelayServerData(new RelayServerData(allocation, protocol));
        _currentLobby.Data.TryGetValue(gameStatus, out DataObject dataObject);

        if(dataObject.Value == Lobby)
        {
            PeriodicallyRefreshLobby();
        }     
    }

    public async Task LockLobby()
    {
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions { IsLocked = true });
        }
        catch (Exception e)
        {
            Debug.Log($"Failed closing lobby: {e}");
        }
    }

    private async void Heartbeat()
    {
        _heartbeatSource = new CancellationTokenSource();
        while (!_heartbeatSource.IsCancellationRequested && _currentLobby != null)
        {
            await Lobbies.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            await UniTask.Delay(HeartbeatInterval * 1000);
        }
    }

    private async void PeriodicallyRefreshLobby()
    {
        _updateLobbySource = new CancellationTokenSource();

        await UniTask.Delay(LobbyRefreshRate * 1000);
        while (!_updateLobbySource.IsCancellationRequested && _currentLobby != null)
        {
            _currentLobby = await Lobbies.Instance.GetLobbyAsync(_currentLobby.Id);
            CurrentLobbyRefreshed?.Invoke(_currentLobby);
            await UniTask.Delay(LobbyRefreshRate * 1000);
        }
    }

    public async UniTask LeaveLobby()
    {
        _heartbeatSource?.Cancel();
        _updateLobbySource?.Cancel();

        if (_currentLobby != null || _currentLobby == null)
        {
            try
            {
                if (_currentLobby.HostId == Authentication.PlayerId)
                {
                    await Lobbies.Instance.DeleteLobbyAsync(_currentLobby.Id);
                }
                                       
                else
                {
                    await Lobbies.Instance.RemovePlayerAsync(_currentLobby.Id, Authentication.PlayerId);
                }
                    
                _currentLobby = null;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public string GetJoinCode()
    {
        return _currentLobby.Data[relayCode].Value;
    }

    public async UniTask<string> JoinLobbyAndAllocationByGameCode(string gameCode)
    {
        string targetLobbyStatus = "";
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions());

        Lobby targetLobby = queryResponse.Results.FirstOrDefault(lobby => 
        lobby.Data.TryGetValue(relayCode, out DataObject dataObject) && dataObject.Value == gameCode);

        if(targetLobby != null) { await JoinLobbyWithAllocation(targetLobby.Id); }

        if(targetLobby.Data.TryGetValue(gameStatus, out DataObject dataObject)) { targetLobbyStatus = dataObject.Value; }

        return targetLobbyStatus;
    }
}