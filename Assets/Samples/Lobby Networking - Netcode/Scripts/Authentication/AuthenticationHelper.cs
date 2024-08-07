using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

/// <summary>
///     This static helper class provides functions for the authentication process.
/// </summary>
public static class AuthenticationHelper
{

    /// <summary>
    ///     Attempts to initialize Unity services
    /// </summary>
    public static async Task InitializeUnityServicesAsync()
    {
        //Ensure we are not already initialized
        if (UnityServices.State != ServicesInitializationState.Uninitialized)
        {
            Debug.Log("UnityServices already initialized");
            return;
        }

        //Setup options variable
        var options = new InitializationOptions();

        //Check if we are a ParrelSync clone so we can use Unity Services correctly
#if UNITY_EDITOR
        if (ClonesManager.IsClone()) options.SetProfile(ClonesManager.GetArgument());
        else options.SetProfile("Primary");
#endif

        try
        {
            //Initialize with UnityServices
            await UnityServices.InitializeAsync(options);
            Debug.Log("Initialization successful");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static async Task SignInAnonymouslyAsync()
    {
        //Check if we are already signed in 
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Already signed in");
            return;
        }

        try
        {
            // If we are not signed in, sign in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in successfully");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}