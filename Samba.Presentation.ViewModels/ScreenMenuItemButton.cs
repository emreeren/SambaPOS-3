using System;
using System.Windows.Input;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Settings;

namespace Samba.Presentation.ViewModels
{
    public class ScreenMenuItemButton
    {
        private readonly ICommand _command;
        private readonly ScreenMenuCategory _category;
        private readonly ScreenMenuItem _screenMenuItem;

        public ScreenMenuItemButton(ScreenMenuItem screenMenuItem, ICommand command, ScreenMenuCategory category)
        {
            _screenMenuItem = screenMenuItem;
            _command = command;
            _category = category;

            var color = screenMenuItem.ButtonColor;

            if (string.IsNullOrEmpty(color))
                color = category != null ? category.MenuItemButtonColor : "Green";

            ButtonColor = color;
        }

        public ScreenMenuItem ScreenMenuItem
        {
            get { return _screenMenuItem; }
        }

        public string Caption
        {
            get
            {
                if (Category.WrapText)
                {
                    if (!_screenMenuItem.Name.Contains("\r")) return _screenMenuItem.Name.Replace(' ', '\r');
                }
                return _screenMenuItem.Name.Replace("\\r", "\r");
            }
        }
        public ICommand Command { get { return _command; } }
        public ScreenMenuCategory Category { get { return _category; } }
        public double ButtonHeight { get { return Category.MenuItemButtonHeight > 0 ? Category.MenuItemButtonHeight : double.NaN; } }
        public string ButtonColor { get; private set; }
        public double FontSize { get { return _screenMenuItem.FontSize > 1 ? _screenMenuItem.FontSize : Category.MenuItemFontSize; } }
        public string ImagePath
        {
            get
            {
                return !string.IsNullOrEmpty(ScreenMenuItem.ImagePath)
                    ? ScreenMenuItem.ImagePath
                    : LocalSettings.AppPath + "\\images\\empty.png";
            }
        }

        public int FindOrder(string t)
        {
            if (Caption.ToLower().StartsWith(t.ToLower())) return -99 + Caption.Length;
            return t.Length == 1 ? Caption.Length : Distance(Caption, t);
        }

        public static int Distance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (var i = 0; i <= n; d[i, 0] = i++) { }
            for (var j = 0; j <= m; d[0, j] = j++) { }

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}
