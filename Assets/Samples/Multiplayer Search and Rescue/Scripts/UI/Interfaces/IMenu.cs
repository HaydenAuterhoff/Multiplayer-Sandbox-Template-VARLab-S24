using UnityEngine.UIElements;

namespace VARLab.Sandbox.SAR.UI
{
    /// <summary>
    ///     An interface to define a menu controlled by a UI document
    /// </summary>
    public interface IMenu
    {
        VisualElement RootElement { get; }

        void Show();

        void Hide();

        void Reset();
    }
}