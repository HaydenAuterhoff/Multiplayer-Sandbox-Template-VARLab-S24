using UnityEngine;
using UnityEngine.UIElements;

public class AuthenticationMenuUiEvents : MonoBehaviour
{
    //Variables
    private UIDocument document;
    private Button button;

    /// <summary>
    /// Awake method
    /// </summary>
    private void Awake()
    {
        //Get the UI Document attached to this script
        document = GetComponent<UIDocument>();

        //Get the authenticate button from the UI Document, and register a click event
        button = document.rootVisualElement.Q("AuthenticateBtn") as Button;
        button.RegisterCallback<ClickEvent>(OnAuthenticateButtonClick);
    }


 /// <summary>
    /// Called when the Authenticate button on the Authenticate scene is clicked
    /// </summary>
    /// <param name="evt">The client event</param>
    private async void OnAuthenticateButtonClick(ClickEvent evt)
    {
        await AuthenticationHelper.InitializeUnityServicesTask();
        await AuthenticationHelper.SignInAnonymouslyTask();
    }
}