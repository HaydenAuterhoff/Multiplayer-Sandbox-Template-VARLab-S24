using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyOrchestrator : NetworkBehaviour
{
    [SerializeField] private MainLobbyScreen mainLobbyScreen;
    [SerializeField] private CreateLobbyScreen createLobbyScreen;
    [SerializeField] private RoomScreen roomScreen;

    [SerializeField] private TMP_Text _errorText;

    private void Start()
    {
        CreateLobbyScreen.LobbyCreated += CreateLobby;
        LobbyRoomPanel.LobbySelected += OnLobbySelected;
        RoomScreen.LobbyLeft += OnLobbyLeft;
        RoomScreen.StartPressed = OnGameStart;
        MainLobbyScreen.JoinLobby += JoinLobby;

        //No ApprovalConnection warning
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck; 

        //NetworkObject.DestroyWithScene = true;
    }

    private async void OnLobbySelected(Lobby lobby)
    {
        try
        {
            await MatchmakingService.Instance.JoinLobbyWithAllocation(lobby.Id);

            JoinLobby();
            
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _errorText.text = "Failed joining lobby, try the refresh button";
            _errorText.color = Color.red;
            _errorText.gameObject.SetActive(true);
            StartCoroutine(HideErrorText());
        }
    }

    IEnumerator HideErrorText()
    {
        yield return new WaitForSeconds(5);
        _errorText.gameObject.SetActive(false);
    }

    public void JoinLobby()
    {
        mainLobbyScreen.gameObject.SetActive(false);
        roomScreen.gameObject.SetActive(true);
        NetworkManager.Singleton.StartClient();
    }

    private async void CreateLobby(CreateLobbyScreen.LobbyData data)
    {
        try
        {
            await MatchmakingService.Instance.CreateLobbyWithAllocation(data);

            createLobbyScreen.gameObject.SetActive(false);
            roomScreen.gameObject.SetActive(true);

            // Starting the host immediately will keep the relay server alive
            NetworkManager.Singleton.StartHost();

        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    #region Room

    private readonly Dictionary<ulong, bool> playersInRoomLobby = new();
    public static event Action<Dictionary<ulong, bool>> RoomLobbyPlayersUpdated;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            playersInRoomLobby.Add(NetworkManager.LocalClientId, false);
            //playersInRoomLobby.Add(ulong.Parse(PlayerNameInput.DisplayName), false);
            UpdateInterface();
        }

        //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        //Client uses this in case host destroys the lobby
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.Pending = false;
    }

    private void OnClientConnectedCallback(ulong playerId)
    {
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

    private void OnClientDisconnectCallback(ulong playerId)
    {
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
            roomScreen.gameObject.SetActive(false);
            mainLobbyScreen.gameObject.SetActive(true);
            OnLobbyLeft();
        }
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

    public void OnReadyClicked()
    {
        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerId)
    {
        playersInRoomLobby[playerId] = true;
        PropagateToClients();
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        RoomLobbyPlayersUpdated?.Invoke(playersInRoomLobby);
    }

    private async void OnLobbyLeft() // Invoked when a player leaves / disconnects
    {
        playersInRoomLobby.Clear();
        NetworkManager.Singleton.Shutdown();
        await MatchmakingService.Instance.LeaveLobby();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CreateLobbyScreen.LobbyCreated -= CreateLobby;
        LobbyRoomPanel.LobbySelected -= OnLobbySelected;
        RoomScreen.LobbyLeft -= OnLobbyLeft;
        RoomScreen.StartPressed -= OnGameStart;

        //Only during the room lobby
        if (NetworkManager.Singleton != null) 
        { 
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback; 
        }
        else  //The Host has diconnected so there is no NetworkManager, but we still need to delete the Lobby
        {
            _ = MatchmakingService.Instance.LeaveLobby(); 
        }
    }

    private async void OnGameStart()
    {
        await MatchmakingService.Instance.LockLobby();
        NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    #endregion

}
