using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

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