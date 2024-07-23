using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    [SerializeField] public GameObject lobbyCanvas;
    public async void LoginAnonymously()
    {
        await Authentication.Login();
        Debug.Log("Signed in anonymously");

        lobbyCanvas.SetActive(true); // This can be a scene change as well
    }
}
