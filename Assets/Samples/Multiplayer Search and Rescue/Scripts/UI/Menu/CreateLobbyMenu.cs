using System;
using UnityEngine.UIElements;
using VARLab.Multiplayer.Lobbies;

namespace VARLab.Sandbox.SAR.UI
{

    public class CreateLobbyMenu : Menu
    {
        private const int DefaultPlayerCount = 4;

        // this will be auto-determined using funny keywords
        private readonly TextField roomNameField;
        private readonly IntegerField maxPlayersField;
        private readonly Button buttonCreate;
        private readonly Button buttonRandom;
        private readonly Button buttonBack;

        public Action Cancelled;
        public Action<LobbyRequestEventArgs> Created;

        private readonly LobbyRequestEventArgs createRequestCache;

        public CreateLobbyMenu(VisualElement root, LobbyRequestEventArgs createRequest) : base(root)
        {
            createRequestCache = createRequest;

            buttonCreate = root.Q<Button>("buttonCreate");
            buttonRandom = root.Q<Button>("buttonRandom");
            buttonBack = root.Q<Button>("buttonBack");

            roomNameField = root.Q<TextField>("roomNameField");
            maxPlayersField = root.Q<IntegerField>("maxPlayersField");

            buttonCreate.clicked += TryCreateRoom;
            buttonRandom.clicked += GenerateRandomRoomName;
            buttonBack.clicked += () => Cancelled?.Invoke();

            createRequestCache.Error += HandleError;
        }

        public override void Reset()
        {
            maxPlayersField.value = DefaultPlayerCount;
            GenerateRandomRoomName();
        }

        public override void Show()
        {
            base.Show();
            Reset();
            SetButtonActionsEnabled(true);
        }

        public void SetButtonActionsEnabled(bool enabled)
        {
            buttonBack.SetEnabled(enabled);
            buttonCreate.SetEnabled(enabled);
            buttonRandom.SetEnabled(enabled);
        }

        public void GenerateRandomRoomName()
        {
            roomNameField.value = RandomNameGenerator.GetRandomName(length: 3, spaces: true);
        }

        private void TryCreateRoom()
        {
            SetButtonActionsEnabled(false);

            string roomName = roomNameField.text.Trim();
            int maxPlayers = maxPlayersField.value;
            // Client-side validation
            if (roomName.Equals(string.Empty))
            {
                UnityEngine.Debug.LogWarning("Room name cannot be empty");
                return;
            }

            if (maxPlayers < 1 || maxPlayers > 12)
            {
                UnityEngine.Debug.LogWarning("Room capacity must be between 1 and 12");
                maxPlayersField.value = DefaultPlayerCount;
                return;
            }

            createRequestCache.LobbyData = new(roomName, maxPlayers);

            // Attempt to create room with server
            // If there is an error, it will be handled by this screen. 
            // The parent screen will handle success callback
            Created?.Invoke(createRequestCache);
        }

        private void HandleError(string error)
        {
            // might print error message to the screen
            SetButtonActionsEnabled(true);
        }
    }
}