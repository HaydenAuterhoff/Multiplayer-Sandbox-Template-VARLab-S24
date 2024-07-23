using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System.Linq;
using Unity.Netcode;

public class RoomScreen : MonoBehaviour
{
    [SerializeField] private LobbyPlayerPanel playerPanelPrefab;
    [SerializeField] private Transform playerPanelParent;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private GameObject startButton, readyButton;

    private readonly List<LobbyPlayerPanel> playerPanels = new();
    private bool allReady;
    private bool isReady;

    public static Action StartPressed;

    #region Event Listeners (OnEnable and OnDisable)
    private void OnEnable()
    {
        foreach (Transform child in playerPanelParent)
        {
            Destroy(child.gameObject);
        }

        playerPanels.Clear();

        LobbyOrchestrator.RoomLobbyPlayersUpdated += NetworkRoomLobbyPlayersUpdated;
        MatchmakingService.Instance.CurrentLobbyRefreshed += OnCurrentRoomLobbyRefreshed;
        
        startButton.SetActive(false);
        readyButton.SetActive(false);

        string joinCode = MatchmakingService.Instance.GetJoinCode();
        joinCodeText.text = $"Join code: {joinCode}";

        isReady = false;
    }

    private void OnDisable()
    {
        LobbyOrchestrator.RoomLobbyPlayersUpdated -= NetworkRoomLobbyPlayersUpdated;
        MatchmakingService.Instance.CurrentLobbyRefreshed -= OnCurrentRoomLobbyRefreshed;
    }

    #endregion

    #region Room Networking

    private void NetworkRoomLobbyPlayersUpdated(Dictionary<ulong, bool> players)
    {
        Debug.Log("NetworkLobbyPlayersUpdated (RoomScreen)");
        var allActivePlayerIds = players.Keys;

        //Remove all inactive panels
        var toDestroy = playerPanels.Where(p => !allActivePlayerIds.Contains(p.PlayerID)).ToList();
        foreach (var panel in toDestroy)
        {
            playerPanels.Remove(panel);
            Destroy(panel.gameObject);
        }

        foreach (var player in players)
        {
            var currentPanel = playerPanels.FirstOrDefault(p => p.PlayerID == player.Key);

            if (currentPanel != null)
            {
                if (player.Value)
                {
                    currentPanel.SetReady();
                }
            }
            else
            {
                var panel = Instantiate(playerPanelPrefab, playerPanelParent);
                panel.Initalize(player.Key);
                playerPanels.Add(panel);
            }
        }

        startButton.SetActive(NetworkManager.Singleton.IsHost && players.All(p => p.Value));
        readyButton.SetActive(!isReady);
    }

    private void OnCurrentRoomLobbyRefreshed(Lobby lobby)
    {
        waitingText.text = $"Waiting on players... {lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void OnReadyClicked()
    {
        readyButton.SetActive(false);
        isReady = false;
    }

    public void OnStartClicked()
    {
        StartPressed?.Invoke();
    }

    #endregion

    #region Leave Lobby

    public static event Action LobbyLeft;

    public void OnLeaveLobby()
    {
        LobbyLeft?.Invoke();
    }

    #endregion

}
