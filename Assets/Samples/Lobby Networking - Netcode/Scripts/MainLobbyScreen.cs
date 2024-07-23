using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Unity.Services.Lobbies.Models;
using System;
public class MainLobbyScreen : MonoBehaviour
{
    [SerializeField] private LobbyRoomPanel lobbyPanelPrefab;
    [SerializeField] private Transform lobbyParent;
    [SerializeField] private GameObject noLobbiesText;
    [SerializeField] private Button lobbyRefreshButton;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TMP_InputField gameCodeText;

    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    private readonly List<LobbyRoomPanel> _currentLobbySpawns = new();
    public static event Action JoinLobby;

    private void OnEnable()
    {
        foreach (Transform child in lobbyParent)
        {
            Destroy(child.gameObject);
        }
        _currentLobbySpawns.Clear();

        Invoke("OnRefreshClick", 0.5f); //Have any lobbies instiated on load (Better UX for host disconnects)
    }

    public async void OnRefreshClick()
    {
        var allLobbies = await MatchmakingService.Instance.GatherLobbies();
        
        ClearLobbies(allLobbies);
        UpdateLobbiesScreen(allLobbies);

        //if (allLobbies.Count > 0) // if you don't clear the lobbies, disconnected hosts rooms are not removed
        //{
        //    //ClearLobbies(allLobbies);
        //    //UpdateLobbiesScreen(allLobbies);
        //}

        noLobbiesText.SetActive(!_currentLobbySpawns.Any());
        lobbyRefreshButton.gameObject.SetActive(false);
        ButtonCooldown().Forget();   
    }

    public async void OnJoinByGameCodeClicked()
    {
        if (string.IsNullOrEmpty(gameCodeText.text) || gameCodeText.text.Length < 6)
        {
            Debug.LogWarning("A valid game code was not provided");
            return;
        }
        try
        {
            string result = await MatchmakingService.Instance.JoinLobbyAndAllocationByGameCode((gameCodeText.text).ToUpper().Replace(" ", ""));
            if (result == "Lobby") { JoinLobby?.Invoke(); }
        }
        catch(Exception) {
            Debug.LogError($"Unable to join the lobby with given code: {gameCodeText.text.ToUpper().Replace(" ", "")}. Please ensure " +
            $"it is the correct code"); }
        gameCodeText.text = "";
    }

    private void UpdateLobbiesScreen(List<Lobby> allLobbies)
    {
        // Update or spawn the remaining active lobbies
        foreach (var lobby in allLobbies)
        {
            string joinCode = lobby.Data["relayCode"].Value;
            var current = _currentLobbySpawns.FirstOrDefault(p => p.Lobby.Id == lobby.Id);
            if (current != null)
            {
                current.DisplayJoinCode(joinCode);
                current.UpdateDetails(lobby);
            }
            else
            {
                var panel = Instantiate(lobbyPanelPrefab, lobbyParent);
                panel.DisplayJoinCode(joinCode);
                panel.UpdateDetails(lobby);
                _currentLobbySpawns.Add(panel);
            }
        }
    }

    private void ClearLobbies(List<Lobby> allLobbies)
    {
        // Destroy all the current lobby panels which don't exist anymore.
        // Exclude our own homes as it'll show for a brief moment after closing the room
        var lobbyIds = allLobbies.Where(l => l.HostId != Authentication.PlayerId).Select(l => l.Id);
        var notActive = _currentLobbySpawns.Where(l => !lobbyIds.Contains(l.Lobby.Id)).ToList();

        foreach (var panel in notActive)
        {
            Destroy(panel.gameObject);
            _currentLobbySpawns.Remove(panel);
        }
    }

    private async UniTask ButtonCooldown()
    {
        stopwatch.Restart();
        while (stopwatch.Elapsed.TotalSeconds < 3)
        {
            int timeLeft = 3 - (int)stopwatch.Elapsed.TotalSeconds;
            timerText.text = $"Please wait {timeLeft} second(s)";
            await UniTask.Yield(PlayerLoopTiming.Update); // Ensures it updates with the main thread
        }
        stopwatch.Stop();
        lobbyRefreshButton.gameObject.SetActive(true);
        timerText.text = "";
    }
}