using System;
using System.Collections.Generic;

namespace VARLab.Multiplayer.Lobbies
{

    public class LobbyListRequestEventArgs : EventArgs
    {
        public IList<LobbyData> Lobbies;

        /// <summary>
        ///     A list of the simplified LobbyData struct in order to provide 
        ///     the least amount of data possible to other services (ie UI)
        /// </summary>
        public Action<IList<LobbyData>> Success;
        public Action<string> Error;

        public LobbyListRequestEventArgs(string options = null,
            Action<IList<LobbyData>> successCallback = null, Action<string> errorCallback = null)
        {
            // options is unused

            Success = successCallback;
            Error = errorCallback;
        }
    }
}