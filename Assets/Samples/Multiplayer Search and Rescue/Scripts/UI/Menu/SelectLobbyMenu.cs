using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VARLab.Multiplayer.Lobbies;

namespace VARLab.Sandbox.SAR.UI
{
    public class SelectLobbyMenu : Menu
    {
        private readonly LobbyListRequestEventArgs listRequestCache;
        private readonly LobbyRequestEventArgs joinRequestCache;

        private readonly float cardHeight;
        private readonly VisualTreeAsset lobbyCard;

        private readonly ListView listView;
        private readonly Button buttonCreate;
        private readonly Button buttonRefresh;
        private readonly Button buttonJoin;

        public Action CreateMenuNavigated;
        public Action<LobbyListRequestEventArgs> RefreshRequested;
        public Action<LobbyRequestEventArgs> JoinRequested;

        private List<LobbyData> Lobbies;

        private float lastRefreshTime = 0f;
        private const float RefreshTimeInterval = 2f;

        public SelectLobbyMenu(VisualElement root, VisualTreeAsset lobbyCard, float cardHeight, 
            LobbyListRequestEventArgs listRequest, LobbyRequestEventArgs joinRequest) : base(root)
        {
            this.lobbyCard = lobbyCard;
            this.cardHeight = cardHeight;
            joinRequestCache = joinRequest;
            listRequestCache = listRequest;

            // Buttons
            buttonCreate = root.Q<Button>("buttonCreate");
            buttonJoin = root.Q<Button>("buttonJoin");
            buttonRefresh = root.Q<Button>("buttonRefresh");

            buttonCreate.clicked += () => CreateMenuNavigated?.Invoke();
            buttonJoin.clicked += TryJoinRoom;
            buttonRefresh.clicked += TryRefresh;


            // ListView
            listView = root.Q<ListView>("roomListView");

            listView.selectionChanged += (_) => CheckJoinAvailability();

            // Lobby list request callbacks.
            // The 'success' callback is not set in the parent, only in here
            listRequestCache.Success += (data) =>
            {
                SetRoomListContent(data);
                buttonRefresh.SetEnabled(true);
            };

            listRequestCache.Error += (_) =>
            {
                buttonRefresh.SetEnabled(true);
            };
        }

        public override void Reset()
        {
            TryRefresh();
        }

        public override void Show()
        {
            base.Show();
            TryRefresh();
            CheckJoinAvailability();
        }

        private void TryJoinRoom()
        {
            CheckJoinAvailability();

            // Validate selected room
            if (listView.selectedItem is LobbyData data)
            {
                Debug.Log($"Attempting to join room {data.JoinCode}");
                joinRequestCache.LobbyData = data;
                JoinRequested?.Invoke(joinRequestCache);
            }
        }

        /// <summary>
        ///     Attempts to refresh the list of lobbies, only if a refresh has not 
        ///     been performed within the last <see cref="RefreshTimeInterval"/> seconds.
        ///     This prevents rate-limit exceptions from the controller
        /// </summary>
        public void TryRefresh()
        {
            if (Time.time < lastRefreshTime + RefreshTimeInterval) { return; }

            // Could update the UI as well while this operation is executing
            Debug.Log("Refreshed");

            RefreshRequested?.Invoke(listRequestCache);
            lastRefreshTime = Time.time;
        }

        public void SetRoomListContent(IList<LobbyData> lobbies)
        {
            if (lobbies == null)
            {
                Debug.LogWarning("Received null list from callback");
                return;
            }

            Debug.Log($"Callback returned with {lobbies.Count} lobbies");
            Lobbies = lobbies as List<LobbyData>;

            listView.itemsSource = Lobbies;
            listView.fixedItemHeight = cardHeight;

            listView.makeItem = () =>
            {
                var card = lobbyCard.Instantiate();
                return card;
            };
            listView.bindItem = (element, index) =>
            {
                LobbyData lobby = Lobbies[index];
                element.Q<Label>("nameLabel").text = lobby.Name;
                element.Q<Label>("joinCodeLabel").text = lobby.JoinCode;
                element.Q<Label>("playersLabel").text = $"{lobby.Capacity - lobby.Slots} / {lobby.Capacity}";
            };

            listView.Rebuild();
        }


        public void SetPlayerId(string playerId)
        {
            RootElement.Q<Label>("nameLabel").text = $"Logged in as <b>{playerId}</b>";
        }

        private bool CheckJoinAvailability()
        {
            bool canJoin = listView?.selectedItem != null;
            buttonJoin.SetEnabled(canJoin);
            return canJoin;
        }


        #region Debug
        private void AddDemoContent()
        {
            System.Random random = new();
            List<string> content = new();

            for (int i = 0; i < 10; i++)
            {
                content.Add(random.Next().ToString("X"));
            }
            listView.makeItem = () => new Label();
            listView.bindItem = (element, index) =>
            {
                (element as Label).text = content[index];
            };
            listView.style.flexGrow = 1f;
        }



        #endregion

    }
}