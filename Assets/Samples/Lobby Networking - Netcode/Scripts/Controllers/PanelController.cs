using UnityEngine;

public class PanelController : MonoBehaviour
{
    [SerializeField] private GameObject mainLobbyScreenGameObject;
    [SerializeField] private GameObject createLobbyScreenGameObject;
    [SerializeField] private GameObject roomLobbyScreenGameObject;

    private void OnEnable()
    {
        EventManager.OnMainLobbySelected += ShowMainLobby;
        EventManager.OnCreateLobbySelected += ShowCreateLobby;
        EventManager.OnRoomScreenSelected += ShowRoomScreen;
    }

    private void OnDisable()
    {
        EventManager.OnMainLobbySelected -= ShowMainLobby;
        EventManager.OnCreateLobbySelected -= ShowCreateLobby;
        EventManager.OnRoomScreenSelected -= ShowRoomScreen;
    }

    private void ShowRoomScreen()
    {
        mainLobbyScreenGameObject.SetActive(false);
        createLobbyScreenGameObject.SetActive(false);
        roomLobbyScreenGameObject.SetActive(true);
    }

    private void ShowCreateLobby()
    {
        mainLobbyScreenGameObject.SetActive(false);
        createLobbyScreenGameObject.SetActive(true);
        roomLobbyScreenGameObject.SetActive(false);
    }

    private void ShowMainLobby()
    {
        mainLobbyScreenGameObject.SetActive(true);
        createLobbyScreenGameObject.SetActive(false);
        roomLobbyScreenGameObject.SetActive(false);
    }
}
