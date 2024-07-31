using System;
using UnityEngine.UIElements;
using VARLab.Multiplayer.Lobbies;

namespace VARLab.Sandbox.SAR.UI
{

    public class CreateLobbyMenu : Menu
    {

        // this will be auto-determined using funny keywords
        private readonly TextField roomNameField;
        private readonly IntegerField maxPlayersField;
        private readonly Button buttonCreate;
        private readonly Button buttonBack;

        public Action Cancelled;
        public Action<LobbyRequestEventArgs> Created;

        private readonly LobbyRequestEventArgs createRequestCache;

        public CreateLobbyMenu(VisualElement root, LobbyRequestEventArgs createRequest) : base(root)
        {
            createRequestCache = createRequest;

            buttonCreate = root.Q<Button>("buttonCreate");
            buttonBack = root.Q<Button>("buttonBack");

            roomNameField = root.Q<TextField>("roomNameField");
            maxPlayersField = root.Q<IntegerField>("maxPlayersField");

            buttonCreate.clicked += TryCreateRoom;
            buttonBack.clicked += () => Cancelled?.Invoke();

            createRequestCache.Error += HandleError;
        }

        public override void Reset()
        {
            roomNameField.value = string.Empty;
            maxPlayersField.value = 1;
        }

        public override void Show()
        {
            base.Show();
            SetButtonActionsEnabled(true);
        }

        public void SetButtonActionsEnabled(bool enabled)
        {
            buttonBack.SetEnabled(enabled);
            buttonCreate.SetEnabled(enabled);
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

            if (maxPlayers < 1)
            {
                UnityEngine.Debug.LogWarning("Room capacity must be positive");
                maxPlayersField.value = 1;
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