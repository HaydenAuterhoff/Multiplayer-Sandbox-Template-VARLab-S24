using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using VARLab.Multiplayer.Lobbies;

namespace VARLab.Sandbox.SAR.UI
{
    public class LobbyMenuManager : MonoBehaviour
    {
        public string DefaultUsername = "Player 1";
        public bool RequireLogin = true;

        [Header("Cards")]
        public VisualTreeAsset LobbyRoomCard;
        public int LobbyRoomCardHeight = 140;
        public VisualTreeAsset PlayerLobbyCard;
        public int PlayerLobbyCardHeight = 92;

        [Header("Events")]
        public UnityEvent<LobbyListRequestEventArgs> LobbyListRequest;
        public UnityEvent<LobbyRequestEventArgs> LobbyCreatedRequest;
        public UnityEvent<LobbyRequestEventArgs> LobbyJoinRequest;
        public UnityEvent<LobbyRequestEventArgs> LobbyLeaveRequest;
        public UnityEvent<LobbyRequestEventArgs> GameStartRequest;

        private readonly List<IMenu> menus = new();
        private IMenu current;

        // Menus
        private LoginMenu loginMenu;
        private SelectLobbyMenu selectLobbyMenu;
        private CreateLobbyMenu createLobbyMenu;
        private LobbyRoomMenu lobbyRoomMenu;

        private UIDocument document;
        private VisualElement root;

        private LobbyListRequestEventArgs listRequestCache;
        private LobbyRequestEventArgs createRequestCache;
        private LobbyRequestEventArgs joinRequestCache;
        private LobbyRequestEventArgs leaveRequestCache;
        private LobbyRequestEventArgs startRequestCache;

        private void OnEnable()
        {
            ConfigureCallbacks();
            ConfigureMenus();

            HideMenus();
        }


        /// <summary>
        ///     Lobby Menu is hidden until receiving a signal from the startup sequence
        /// </summary>
        private void Start()
        {
            Hide();
        }

        /// <summary>
        ///     Defines the various event argument callbacks for querying lobby room data.
        ///     Each ...RequestEventArgs defines a success callback with returned data and
        ///     an error callback with an error message as a string.
        /// </summary>
        private void ConfigureCallbacks()
        {
            createRequestCache = new(null, DisplayLobbyRoom, DefaultErrorCallback);
            joinRequestCache = new(null, DisplayLobbyRoom, DefaultErrorCallback);
            leaveRequestCache = new(null, (_) => SwitchMenu(selectLobbyMenu), DefaultErrorCallback);
            listRequestCache = new(null, null, DefaultErrorCallback);
            startRequestCache = new(null, (_) => Hide(), GameStartErrorCallback);
        }


        private void ConfigureMenus()
        {
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;

            // Configure Login menu
            loginMenu = new(root.Q<VisualElement>("loginMenu"));
            loginMenu.Login += (_) => SwitchMenu(selectLobbyMenu);
            menus.Add(loginMenu);


            // Configure 'Select Lobby' menu
            selectLobbyMenu = new(root.Q<VisualElement>("selectLobbyMenu"),
                LobbyRoomCard, LobbyRoomCardHeight,
                listRequestCache, joinRequestCache);
            selectLobbyMenu.CreateMenuNavigated += () => SwitchMenu(createLobbyMenu);
            selectLobbyMenu.JoinRequested += SendLobbyJoinRequest;
            selectLobbyMenu.RefreshRequested += SendLobbyListRequest;
            menus.Add(selectLobbyMenu);


            // Configure 'Create Lobby' menu
            createLobbyMenu = new(root.Q<VisualElement>("createLobbyMenu"), createRequestCache);
            createLobbyMenu.Cancelled += () => SwitchMenu(selectLobbyMenu);
            createLobbyMenu.Created += SendLobbyCreateRequest;
            menus.Add(createLobbyMenu);


            // Configure 'Lobby Room' menu
            lobbyRoomMenu = new(root.Q<VisualElement>("lobbyRoomMenu"),
                PlayerLobbyCard, PlayerLobbyCardHeight,
                startRequestCache, leaveRequestCache);
            lobbyRoomMenu.Cancelled += SendLobbyLeaveRequest;
            lobbyRoomMenu.Started += SendGameStartRequest;
            menus.Add(lobbyRoomMenu);
        }

        public void HandleLogin(string playerId)
        {
            selectLobbyMenu.SetPlayerId(playerId);
            SwitchMenu(selectLobbyMenu);
            Show();
        }

        public void Show()
        {
            root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            root.style.display = DisplayStyle.None;
        }

        public void SwitchMenu(IMenu target)
        {
            if (target == null)
            {
                Debug.Log("Attempted to select a null menu target");
                return;
            }

            current?.Hide();
            current = target;
            current?.Show();
        }

        public void HideMenus()
        {
            menus.ForEach(item => item?.Hide());
        }


        // Controller event handlers

        public void UpdateLobbyData(LobbyData data)
        {
            if (data == null || data.Id == null || data.Id == string.Empty)
            {
                Debug.Log("Invalid Lobby Data provided. Switching to selection screen.");
                SwitchMenu(selectLobbyMenu);
                return;
            }

            lobbyRoomMenu.SetActiveRoom(data);
        }

        // TODO validate if this is necessary, since the periodic lobby refresh passes a LobbyData object 
        // containing the list of players
        public void UpdateLobbyRoomPlayers(Dictionary<ulong, bool> playerIds)
        {

        }


        // Request invokations

        public void SendLobbyListRequest(LobbyListRequestEventArgs args)
        {
            LobbyListRequest?.Invoke(args);
        }

        public void SendLobbyCreateRequest(LobbyRequestEventArgs args)
        {
            LobbyCreatedRequest?.Invoke(args);
        }

        public void SendLobbyJoinRequest(LobbyRequestEventArgs args)
        {
            LobbyJoinRequest?.Invoke(args);
        }

        public void SendLobbyLeaveRequest(LobbyRequestEventArgs args)
        {
            LobbyLeaveRequest?.Invoke(args);
        }

        public void SendGameStartRequest(LobbyRequestEventArgs args)
        {
            GameStartRequest?.Invoke(args);
        }


        // Request callbacks

        public void DisplayLobbyRoom(LobbyData data)
        {
            lobbyRoomMenu.SetActiveRoom(data);
            SwitchMenu(lobbyRoomMenu);
        }

        private void GameStartErrorCallback(string error)
        {
            // Tell LobbyRoomMenu to reset buttons
            Debug.LogError("Error received when attempting to start game");
        }

        public void DefaultErrorCallback(string error)
        {
            // TODO print friendly error to UI 
            Debug.LogError($"TODO print friendly error to UI : {error}");
        }
    }
}