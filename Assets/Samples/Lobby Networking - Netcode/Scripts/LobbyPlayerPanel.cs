using UnityEngine;
using TMPro;

public class LobbyPlayerPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, statusText;

    public ulong PlayerID { get; private set; }

    public void Initalize(ulong playerID)
    {
        PlayerID = playerID;
        nameText.text = $"Player: {playerID}";
    }

    public void SetReady()
    {
        statusText.text = "Ready";
        statusText.color = Color.green;
    }
}
