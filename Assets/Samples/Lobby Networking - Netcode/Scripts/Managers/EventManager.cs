using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnMainLobbySelected;
    public static event Action OnCreateLobbySelected;
    public static event Action OnRoomScreenSelected;

    public static void ShowMainLobbyScreen() => OnMainLobbySelected?.Invoke();
    public static void ShowCreateLobbyScreen() => OnCreateLobbySelected?.Invoke();
    public static void ShowRoomScreen() => OnRoomScreenSelected?.Invoke();
}
