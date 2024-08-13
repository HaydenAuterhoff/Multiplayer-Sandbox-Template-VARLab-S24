using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using VARLab.Multiplayer.Lobbies;

namespace VARLab.Sandbox.SAR.UI
{
    public class LobbyRoomMenu : Menu
    {
        public Action Confirmed;
        public Action<LobbyRequestEventArgs> Started;
        public Action<LobbyRequestEventArgs> Cancelled;

        private readonly float cardHeight;
        private readonly VisualTreeAsset playerCard;

        private readonly Button buttonReady;
        private readonly Button buttonStart;
        private readonly Button buttonBack;

        private readonly ListView playersListView;


        private List<string> players = new();
        private List<PlayerData> Players = new();

        private readonly LobbyRequestEventArgs startRequestCache;
        private readonly LobbyRequestEventArgs leaveRequestCache;

        public LobbyRoomMenu(VisualElement root, VisualTreeAsset playerCard, float cardHeight,
            LobbyRequestEventArgs startRequest, LobbyRequestEventArgs leaveRequest) : base(root)
        {
            this.playerCard = playerCard;
            this.cardHeight = cardHeight;
            startRequestCache = startRequest;
            leaveRequestCache = leaveRequest;

            buttonReady = root.Q<Button>("buttonReady");
            buttonStart = root.Q<Button>("buttonStart");
            buttonBack = root.Q<Button>("buttonBack");

            playersListView = root.Q<ListView>("playersListView");

            buttonStart.SetEnabled(false);
            buttonStart.clicked += () =>
            {
                buttonStart.SetEnabled(false);
                buttonReady.SetEnabled(false);
                buttonBack.SetEnabled(false);
                Started?.Invoke(startRequestCache);
            };
            buttonBack.clicked += () => Cancelled?.Invoke(leaveRequestCache);
            buttonReady.clicked += () => buttonStart.SetEnabled(!buttonStart.enabledSelf);
        }

        // Will need to be periodically refreshed, as notified by the MatchmakingService
        public void SetActiveRoom(LobbyData data)
        {
            UnityEngine.Debug.Log($"Lobby data updated: {data.Name} - {data.Players.Count} players");

            // Update data for Lobby Request
            leaveRequestCache.LobbyData = data;
            startRequestCache.LobbyData = data;


            // UI Setup
            RootElement.Q<Label>("labelRoomCode").text = $"{data.Name} ({data.JoinCode})"; //Here
            RootElement.Q<Label>("labelPlayerCount").text = $"Players {data.Players.Count} of {data.Capacity}";


            Players = data.Players;
            playersListView.itemsSource = Players;
            playersListView.fixedItemHeight = cardHeight;

            playersListView.makeItem = () =>
            {
                var card = playerCard.Instantiate();
                // do other things to connect controller to card
                return card;
            };

            playersListView.bindItem = (element, index) =>
            {
                var player = Players[index];
                element.Q<Label>("nameLabel").text = $"Player {index + 1} (Name: {player.Name})";
            };

            playersListView.Rebuild();
        }





        public void AddDemoContent()
        {
            players = new() { "Alpha", "Beta", "Gamma" };

            playersListView.fixedItemHeight = 92f;

            playersListView.makeItem = () =>
            {
                var card = playerCard.Instantiate();
                // do other things to connect controller to card
                return card;
            };

            playersListView.bindItem = (element, index) =>
            {
                element.Q<Label>("nameLabel").text = players[index];
            };

            playersListView.itemsSource = players;
            playersListView.Rebuild();
        }
    }
}
