using UnityEngine.UIElements;

namespace VARLab.Sandbox.SAR.UI
{

    public abstract class Menu : IMenu
    {
        public VisualElement RootElement { get; }

        public Menu(VisualElement root)
        {
            RootElement = root;
        }

        public virtual void Reset() { }

        public virtual void Show()
        {
            RootElement.style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            RootElement.style.display = DisplayStyle.None;
        }
    }
}