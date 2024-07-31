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

        public UnityEvent<object> NetworkedGameStarted;

        public UnityEvent<Dictionary<ulong, bool>> LobbyRoomPlayersUpdated;

        private readonly Dictionary<ulong, bool> playersInRoomLobby = new();

        private string errorString;

        private List<Lobby> lobbyCache;

        public void Start()
        {
            NetworkObject.DestroyWithScene = true;


            MatchmakingService.Instance.CurrentLobbyRefreshed += (lobby) => LobbyRefreshed?.Invoke(new LobbyData(lobby));

            //No ApprovalConnection warning
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Only during the room lobby
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
            else if (MatchmakingService.Instance)
            {
                //The Host has diconnected so there is no NetworkManager, but we still need to delete the Lobby
                _ = MatchmakingService.Instance.LeaveLobby();
            }
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
            try
            {
                await MatchmakingService.Instance.JoinLobbyWithAllocation(args.LobbyData.Id);

                if (NetworkManager.Singleton.StartClient())
                {
                    // Client start success
                    args.LobbyData = new LobbyData(MatchmakingService.CurrentLobby);
                    args.Success?.Invoke(args.LobbyData);
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
                return;
            }

            if (args.LobbyData.Id != MatchmakingService.CurrentLobby.Id)
            {
                // throw error, invalid lobby
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
                NetworkedGameStarted?.Invoke(this);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                errorString = "Error while starting game.";
            }

            args.Error?.Invoke(errorString);
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.Pending = false;
        }



        #region Room


        public override void OnNetworkSpawn()
        {
            Debug.Log("LobbyOrchestrator.OnNetworkSpawn");

            if (IsServer)
            {
                Debug.Log("Setting up ClientConnectedCallback");
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
                playersInRoomLobby.Add(NetworkManager.LocalClientId, false);
                UpdateInterface();
            }

            //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

            Debug.Log("Setting up ClientDisconnectedCallback");

            //Client uses this in case host destroys the lobby
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }

        private void OnClientConnectedCallback(ulong playerId)
        {
            Debug.Log("Calling ClientConnectedCallback");

            if (!IsServer)
                return;

            //Add locally
            if (!playersInRoomLobby.ContainsKey(playerId))
                playersInRoomLobby.Add(playerId, false);

            PropagateToClients();
            UpdateInterface();
        }

        public void PropagateToClients()
        {
            foreach (var player in playersInRoomLobby)
            {
                UpdatePlayersClientRpc(player.Key, player.Value);
            }
        }



        private void OnClientDisconnectCallback(ulong playerId)
        {
            Debug.Log("Is OnClientDisconnectCallback being called?");

            if (IsServer) //Event Listener is for server --> Handle if disconnect is Server or Clients
            {
                //The Server is still active and contains the player that disconnected/Left --> Remove player
                if (playersInRoomLobby.ContainsKey(playerId))
                {
                    playersInRoomLobby.Remove(playerId);
                }
                else // The Server is active but has left
                {
                    OnLobbyLeft();
                }

                //Propagate information to all clients
                RemovePlayerClientRpc(playerId);

                UpdateInterface();
            }
            else //Event Listener for a client --> Handles the client that disconnects
            {
                //roomScreen.gameObject.SetActive(false);
                //mainLobbyScreen.gameObject.SetActive(true);
                OnLobbyLeft();
            }
        }



        public void OnReadyClicked()
        {
            SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }



        private void UpdateInterface()
        {
            Debug.Log("Attempting to 'update interface'");
            LobbyRoomPlayersUpdated?.Invoke(playersInRoomLobby);
        }

        [Obsolete("This function is likely redundant now. Old code has been commented out")]
        private async void OnLobbyLeft() // Invoked when a player leaves / disconnects
        {
            Debug.LogWarning("OnLobbyLeft is being called? But we've already left the lobby and shutdown the NetworkManager");
            return;
            //playersInRoomLobby.Clear();
            //await MatchmakingService.Instance.LeaveLobby();
            //NetworkManager.Singleton.Shutdown();
        }




        [ServerRpc(RequireOwnership = false)]
        private void SetReadyServerRpc(ulong playerId)
        {
            playersInRoomLobby[playerId] = true;
            PropagateToClients();
            UpdateInterface();
        }

        [ClientRpc]
        private void RemovePlayerClientRpc(ulong clientId)
        {
            if (IsServer)
                return;

            if (playersInRoomLobby.ContainsKey(clientId))
                playersInRoomLobby.Remove(clientId);

            UpdateInterface();
        }

        [ClientRpc]
        private void UpdatePlayersClientRpc(ulong clientId, bool isReady)
        {
            if (IsServer)
                return;

            if (!playersInRoomLobby.ContainsKey(clientId))
            {
                playersInRoomLobby.Add(clientId, isReady);
            }
            else
                playersInRoomLobby[clientId] = isReady;

            UpdateInterface();
        }

        #endregion

    }
}