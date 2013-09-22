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
            return (BackButton ? "< " : "") + Name.Split(',').Last().Trim().Replace("\\r", "\r");
        }

        public ICommand Command { get; set; }
        public int Height { get; set; }
        public double FontSize { get; set; }
        public bool BackButton { get; set; }

        public ScreenSubCategoryButton(string name, ICommand command, string buttonColor, double fontSize, int height, bool backButton = false)
        {
            Name = name;
            Command = command;
            Height = height;
            FontSize = fontSize;
            BackButton = backButton;
            ButtonColor = GetButtonColor(buttonColor, name);
        }

        private string GetButtonColor(string buttonColor, string name)
        {
            if (!buttonColor.Contains("=")) return buttonColor;
            var colordef = buttonColor.Split(';').FirstOrDefault(x => x.StartsWith(name + "="));
            if (!string.IsNullOrEmpty(colordef))
            {
                colordef = colordef.Split('=')[1];
            }
            if (string.IsNullOrEmpty(colordef)) colordef = "Gainsboro";
            return colordef;
        }
    }
}