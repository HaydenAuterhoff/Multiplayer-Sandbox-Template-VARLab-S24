using UnityEngine;
using UnityEngine.UIElements;

public class TestMenuLobbyUiEvents : MonoBehaviour
{
    private UIDocument Docuemnt;
    private TextField TextField;
    private Button Button;

    private void Awake()
    {
        Docuemnt = GetComponent<UIDocument>();
        TextField = Docuemnt.rootVisualElement.Q("GameCodeField") as TextField;

        Button = Docuemnt.rootVisualElement.Q("GameCodeButton") as Button;
        Button.RegisterCallback<ClickEvent>(OnGameCodeButtonClicked);

    }

    private async void OnGameCodeButtonClicked(ClickEvent evt)
    {
        string joinCode = TextField.text;
        if (string.IsNullOrEmpty(joinCode) || joinCode.Length != TextField.maxLength) 
        {
            Debug.LogWarning("Must input a game code of length 6");
            return;
        }
       await GameCodeServices.JoinWithGameCode(joinCode);
    }
}
