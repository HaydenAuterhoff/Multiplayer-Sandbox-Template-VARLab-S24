using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Authentication;
using System.Threading.Tasks;

#if UNITY_EDITOR
using ParrelSync;
#endif
using System;

/// <summary>
/// This class is meant to help with anything regarding the authentication process. It is a static class
/// with helper methods, it can be added on to or changed if needed
/// </summary>
public static class AuthenticationHelper
{
    public static async Task InitializeUnityServicesTask()
    {
        //Ensure we are not already initialized
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            //Setup options variable
            var options = new InitializationOptions();

            //Check if we are in the unity editor and using ParrelSync, UnityServices will see them as the same person
            //if not accounted for
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
        else
        {
            Debug.Log("UnityServices already initialized");
        }
    }
    public static async Task SignInAnonymouslyTask()
    {
        //Check if we are singed in already
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                //If we are not signed in, sign in
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in successfully");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        else
        {
            Debug.Log("Already signed in");
        }
    }
}