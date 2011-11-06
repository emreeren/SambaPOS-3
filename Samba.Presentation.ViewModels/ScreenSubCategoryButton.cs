using System;
using System.Linq;
using System.Windows.Input;

namespace Samba.Presentation.ViewModels
{
    public class ScreenSubCategoryButton
    {
        public string Name { get; set; }
        public string Caption { get { return GetCaption(); } }
        public string ButtonColor { get; set; }

        private string GetCaption()
        {
            return (BackButton ? "< " : "") + Name.Split(',').Last().Trim();
        }

        public ICommand Command { get; set; }
        public int Height { get; set; }
        public bool BackButton { get; set; }

        public ScreenSubCategoryButton(string name, ICommand command, string buttonColor, int height, bool backButton = false)
        {
            Name = name;
            Command = command;
            Height = height;
            BackButton = backButton;
            ButtonColor = buttonColor;
        }
    }
}