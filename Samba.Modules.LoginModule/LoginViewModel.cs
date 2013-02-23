using System.ComponentModel.Composition;
using System.IO;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Services;

namespace Samba.Modules.LoginModule
{
    [Export]
    public class LoginViewModel
    {
        private readonly IUserService _userService;

        [ImportingConstructor]
        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
        }

        public string LogoPath
        {
            get
            {
                if (File.Exists(LocalSettings.LogoPath))
                    return LocalSettings.LogoPath;
                if (File.Exists(LocalSettings.DocumentPath + "\\Images\\logo.png"))
                    return LocalSettings.DocumentPath + "\\Images\\logo.png";
                if (File.Exists(LocalSettings.AppPath + "\\Images\\logo.png"))
                    return LocalSettings.AppPath + "\\Images\\logo.png";
                return LocalSettings.AppPath + "\\Images\\empty.png";
            }
            set { LocalSettings.LogoPath = value; }
        }

        public string AppLabel { get { return "SAMBA POS " + LocalSettings.AppVersion + " - " + GetDatabaseLabel(); } }
        public string AdminPasswordHint { get { return GetAdminPasswordHint(); } }
        public string SqlHint { get { return GetSqlHint(); } }

        private string GetSqlHint()
        {
            return !string.IsNullOrEmpty(GetAdminPasswordHint()) ? Resources.SqlHint : "";
        }

        private static string GetDatabaseLabel()
        {
            return LocalSettings.DatabaseLabel;
        }

        public string GetAdminPasswordHint()
        {
            if ((GetDatabaseLabel() == "TX" || GetDatabaseLabel() == "CE") && _userService.IsDefaultUserConfigured)
            {
                return Resources.AdminPasswordHint;
            }

            return "";
        }
    }
}
