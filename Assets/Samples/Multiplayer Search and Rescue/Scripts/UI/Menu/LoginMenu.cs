using System;
using UnityEngine.UIElements;

namespace VARLab.Sandbox.SAR.UI
{
    public class LoginMenu : Menu
    {
        private readonly Button buttonLogin;
        private readonly TextField nameField;

        public Action<string> Login;

        public LoginMenu(VisualElement root) : base(root)
        {
            nameField = root.Q<TextField>("nameField");
            buttonLogin = root.Q<Button>("buttonLogin");

            buttonLogin.clicked += () => Login?.Invoke(nameField.text);
        }

        public override void Reset()
        {
            nameField.value = string.Empty;
        }
    }
}