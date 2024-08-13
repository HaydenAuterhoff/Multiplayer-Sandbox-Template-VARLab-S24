using Unity.Services.Lobbies.Models;

namespace VARLab.Multiplayer.Lobbies
{

    /// <summary>
    ///     Represents a subset of the data representing a <see cref="Player"/>
    /// </summary>
    public class PlayerData
    {
        public string Id;
        public string Name;

        public PlayerData(Player fullPlayerData)
        {
            Id = fullPlayerData.Id;
            if(fullPlayerData.Data != null)
            {
                Name = fullPlayerData.Data["Name"]?.Value;
            }
        }
    }
}