using System;

namespace VARLab.Multiplayer.Lobbies
{
    public class LobbyRequestEventArgs : EventArgs
    {
        public LobbyData LobbyData;
        public Action<LobbyData> Success;
        public Action<string> Error;

        public LobbyRequestEventArgs(string name, int capacity = 1,
            Action<LobbyData> successCallback = null, Action<string> errorCallback = null)
        : this(new LobbyData(name, capacity), successCallback, errorCallback) { }

        public LobbyRequestEventArgs(Action<LobbyData> successCallback = null,
            Action<string> errorCallback = null)
        : this(null, successCallback, errorCallback) { }

        public LobbyRequestEventArgs(LobbyData lobbyData = null,
            Action<LobbyData> successCallback = null, Action<string> errorCallback = null)
        {
            LobbyData = lobbyData;
            Success = successCallback;
            Error = errorCallback;
        }
    }
}