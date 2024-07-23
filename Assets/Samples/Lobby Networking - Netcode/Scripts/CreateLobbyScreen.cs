using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyScreen : MonoBehaviour
{
    public struct LobbyData
    {
        public string Name;
        public int MaxPlayers;
        public string JoinCode;
    }

    [SerializeField] private TMP_InputField nameInput, maxPlayersInput;
    [SerializeField] private Button createLobbyButton;

    public static event Action<LobbyData> LobbyCreated; //Listener Event in LobbyOrchestrator

    private void OnEnable()
    {
        createLobbyButton.interactable = true;
    }

    public void OnCreateClicked()
    {
        createLobbyButton.interactable = false;
        var lobbyData = new LobbyData
        {
            Name = nameInput.text,
            MaxPlayers = int.Parse(maxPlayersInput.text)
        };

        LobbyCreated?.Invoke(lobbyData);
    }
}
