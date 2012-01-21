using System.Windows.Input;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Settings;

namespace Samba.Presentation.ViewModels
{
    public class ScreenCategoryButton
    {
        private readonly ScreenMenuCategory _category;

        public ScreenCategoryButton(ScreenMenuCategory category, ICommand command)
        {
            _category = category;
            ButtonCommand = command;
        }

        public ScreenMenuCategory Category { get { return _category; } }
        public string Caption { get { return Category.Name.Replace("\\r", "\r"); } }
        public ICommand ButtonCommand { get; private set; }
        public double FontSize { get { return _category.MainFontSize; } }
        public string MButtonColor { get { return _category.MainButtonColor; } }
        public double MButtonHeight { get { return _category.MainButtonHeight > 0 ? _category.MainButtonHeight : double.NaN; } }
        public string ImagePath { get { return !string.IsNullOrEmpty(Category.ImagePath) ? Category.ImagePath : LocalSettings.AppPath + "\\images\\empty.png"; } }
    }
}
