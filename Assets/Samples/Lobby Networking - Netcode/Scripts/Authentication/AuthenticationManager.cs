using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;

public class AuthenticationManager : MonoBehaviour
{

    public static string PlayerId { get; private set; }
    public static string DisplayName { get; private set; }

    public UnityEvent<string> LoggedIn;

    public bool AutomaticLogin = false;

    public IEnumerator Start()
    {
        if (AutomaticLogin)
        {
            TryLogin();
        }

        yield break;
    }

    public async void TryLogin()
    {
        try
        {
            await LoginAsync();
            Debug.Log($"PlayerId {PlayerId}");
            Debug.Log("Signed in anonymously");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            PlayerId = null;
        }
        finally
        {
            LoggedIn?.Invoke(PlayerId);
        }
    }


    public async Task LoginAsync()
    {
        await AuthenticationHelper.InitializeUnityServicesAsync();
        await AuthenticationHelper.SignInAnonymouslyAsync();
        PlayerId = AuthenticationService.Instance.PlayerId;
    }

    public static void SetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }
}
