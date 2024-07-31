using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace VARLab.Multiplayer.Lobbies
{

    /// <summary>
    ///     Represents a subset of the data representing a <see cref="Lobby"/>
    /// </summary>
    public class LobbyData
    {
        public string Id;
        public string Name;
        public string JoinCode;
        public int Slots;
        public int Capacity;
        public bool IsPrivate;
        public bool IsLocked;
        public bool HasPassword;
        public List<PlayerData> Players;

        public LobbyData(string name, int capacity = 1)
        {
            Name = name;
            Capacity = capacity;
            JoinCode = string.Empty;
        }

        public LobbyData(Lobby fullLobbyData)
        {
            // Can create a null lobby 
            if (fullLobbyData == null) { return; }

            Id = fullLobbyData.Id;
            Name = fullLobbyData.Name;
            JoinCode = fullLobbyData.LobbyCode;
            Capacity = fullLobbyData.MaxPlayers;
            Slots = fullLobbyData.AvailableSlots;
            IsPrivate = fullLobbyData.IsPrivate;
            IsLocked = fullLobbyData.IsLocked;
            HasPassword = fullLobbyData.HasPassword;
            Players = fullLobbyData.Players.ConvertAll(player => new PlayerData(player));
        }
    }
}