using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyRoomPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, playerCountText, joinCodeText;

    public Lobby Lobby { get; private set; }

    public static event Action<Lobby> LobbySelected;

    public void UpdateDetails(Lobby lobby)
    {
        Lobby = lobby;
        nameText.text = lobby.Name;
        playerCountText.text = $"<b>Players</b> {lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void DisplayJoinCode(string joinCode)
    {
        joinCodeText.text = $"<b>Join code</b> {joinCode}";
    }

    public void Clicked()
    {
        gameObject.SetActive(false);
        Debug.Log("Clicked! Lobby Info: " + Lobby);
        LobbySelected?.Invoke(Lobby);
    }
}
