using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.Multiplayer.Lobbies
{

    /// <summary>
    ///     MonoBehaviour responsible for creating and retrieving available multiplayer rooms
    /// </summary>
    public class LobbyOrchestrator : NetworkBehaviour
    {
        public bool LockLobbyOnStart = false;

        [Header("Events")]
        public UnityEvent<LobbyData> LobbyRefreshed;

        public UnityEvent<LobbyData> LobbyCreated;

        public UnityEvent<LobbyData> LobbyJoined;

        public UnityEvent<LobbyData> LobbyLeft;

        public UnityEvent<object> GameStarted;

        private string errorString;

        private List<Lobby> lobbyCache;

        public void Start()
        {
            MatchmakingService.Instance.CurrentLobbyRefreshed += (lobby) => LobbyRefreshed?.Invoke(new LobbyData(lobby));

            // Define ConnectionApproval when a user joins the lobby
            NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Only during the room lobby
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
            else if (MatchmakingService.Instance)
            {
                //The Host has diconnected so there is no NetworkManager, but we still need to delete the Lobby
                _ = MatchmakingService.Instance.LeaveLobby();
            }
        }


        /// <summary>
        ///     NetworkBehaviour callback executed when network is connected
        /// </summary>
        public override void OnNetworkSpawn()
        {
            Debug.Log("LobbyOrchestrator spawned in network");

            if (IsServer)
            {
                Debug.Log("Setting up ClientConnectedCallback");
                NetworkManager.Singleton.OnClientConnectedCallback += HandleConnectedClient;
            }

            Debug.Log("Setting up ClientDisconnectedCallback");
            //Client uses this in case host destroys the lobby
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        /// <summary>
        ///     Listener for when a client has connected to the server
        /// </summary>
        /// <param name="playerId"></param>
        protected void HandleConnectedClient(ulong playerId)
        {
            Debug.Log($"Client {playerId} has connected");
        }

        /// <summary>
        ///     Listener for when a client has disconnected. 
        ///     All clients should be subscribed.
        /// </summary>
        /// <param name="playerId"></param>
        private void HandleClientDisconnected(ulong playerId)
        {
            // Listener for the server. Handle if disconnect is Server or Clients
            if (IsServer) 
            {
                Debug.Log($"Client {playerId} disconnected from server");
            }
            // Listener fr the client. Handles the client that disconnects
            else 
            {
                Debug.Log($"Client {playerId} disconnected");
            }
        }

        /// <summary>
        ///     Allows for accepting/declining users as they join the lobby. 
        ///     Custom player prefabs can be loaded here
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Debug.Log($"Player {request.ClientNetworkId} connecting...");
            response.PlayerPrefabHash = null;
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Pending = false;
        }


        /// <summary>
        ///     Attempts to create a new lobby room using the provided event data
        /// </summary>
        /// <param name="args">
        ///     Request event args containing a <see cref="LobbyData"/> struct as well as
        ///     Success and Error callbacks
        /// </param>
        public async void TryCreateLobby(LobbyRequestEventArgs args)
        {
            if (!MatchmakingService.Instance)
            {
                errorString = "No matchmaking service available.";
                Debug.LogWarning(errorString);
                args.Error?.Invoke(errorString);
                return;
            }

            try
            {
                await MatchmakingService.Instance.CreateLobbyWithAllocation(args.LobbyData);

                // Starting the host immediately will keep the relay server alive
                if (NetworkManager.Singleton.StartHost())
                {
                    args.LobbyData = new LobbyData(MatchmakingService.CurrentLobby);
                    Debug.Log($"End of TryCreateLobby with success response. {args.LobbyData.JoinCode} {args.LobbyData.Id}");
                    args.Success?.Invoke(args.LobbyData);

                    // Success event broadcast
                    LobbyCreated?.Invoke(args.LobbyData);
                    return;
                }

                errorString = "Network manager failed to start hosting the room.";
            }
            catch (Exception ex)
            {
                // TODO
                // Obviously should not be printing the actual exception to the UI,
                // but this is for testing
                errorString = ex.Message;
            }

            Debug.LogError(errorString);
            args.Error?.Invoke(errorString);

        }

        public async void TryGetLobbies(LobbyListRequestEventArgs args)
        {
            try
            {
                // If this is too much data, consider parsing into a set of reduced LobbyData class
                lobbyCache = await MatchmakingService.Instance.GatherLobbies();
                Debug.Log($"LobbyOrchestrator returned {lobbyCache.Count} lobbies");

                // Converts the returned set to the set of reduced-data LobbyData structs
                args.Lobbies = lobbyCache.ConvertAll(item => new LobbyData(item));

                args.Success?.Invoke(args.Lobbies);
                return;
            }
            catch (Exception ex)
            {
                errorString = ex.Message;
                Debug.LogError(errorString);
            }

            args.Error?.Invoke(errorString);

        }

        public async void TryJoinLobby(LobbyRequestEventArgs args)
        {
            Debug.Log("Trying to join the lobby...");

            try
            {
                await MatchmakingService.Instance.JoinLobbyWithAllocation(args.LobbyData.Id);

                if (MatchmakingService.CurrentLobby == null)
                {
                    Debug.LogError("Error getting current Lobby");
                }

                if (NetworkManager.Singleton.StartClient())
                {
                    // Client start success
                    Debug.Log("Client start success");

                    args.LobbyData = new LobbyData(MatchmakingService.CurrentLobby);
                    args.Success?.Invoke(args.LobbyData);

                    // Success event broadcast
                    LobbyJoined?.Invoke(args.LobbyData);
                    return;
                }

                errorString = "Client start error when joining lobby.";
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                errorString = "Failed joining lobby. Please try refreshing.";
            }

            args.Error?.Invoke(errorString);

        }

        public async void TryLeaveLobby(LobbyRequestEventArgs args)
        {
            try
            {
                await MatchmakingService.Instance.LeaveLobby();
                NetworkManager.Singleton.Shutdown();
                args.Success?.Invoke(null);

                // Success event broadcast
                LobbyLeft?.Invoke(null);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                errorString = "Error while leaving lobby";
            }

            args.Error?.Invoke(errorString);
        }

        public async void TryStartGame(LobbyRequestEventArgs args)
        {
            if (MatchmakingService.CurrentLobby == null)
            {
                // throw error, no lobby
                Debug.LogWarning("Attempting to start game with no active lobby. This is not a valid action");
                return;
            }

            if (args.LobbyData.Id != MatchmakingService.CurrentLobby.Id)
            {
                // throw error, invalid lobby
                Debug.LogWarning("Game started with incorrect lobby data. This is not a valid action");
                return;
            }

            try
            {
                if (LockLobbyOnStart)
                {
                    await MatchmakingService.Instance.LockLobby();
                }

                Debug.Log("Game started!");
                args.Success?.Invoke(new LobbyData(MatchmakingService.CurrentLobby));
                GameStarted?.Invoke(this);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                errorString = "Error while starting game.";
            }

            args.Error?.Invoke(errorString);
        }
    }
}
