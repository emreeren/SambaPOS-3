using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using Samba.Infrastructure.Helpers;

namespace Samba.Infrastructure.Settings
{
    public class SettingsObject
    {
        public int MessagingServerPort { get; set; }
        public string MessagingServerName { get; set; }
        public string TerminalName { get; set; }
        public string ConnectionString { get; set; }
        public bool StartMessagingClient { get; set; }
        public string LogoPath { get; set; }
        public string DefaultHtmlReportHeader { get; set; }
        public string CurrentLanguage { get; set; }
        public bool OverrideLanguage { get; set; }
        public bool OverrideWindowsRegionalSettings { get; set; }
        public int DefaultRecordLimit { get; set; }
        public double WindowScale { get; set; }
        public bool AllowMultipleClients { get; set; }
        public string ApiHost { get; set; }
        public string ApiPort { get; set; }
        public TimeSpan TokenLifeTime { get; set; }

        public string PrintFontFamily { get; set; }

        private readonly SerializableDictionary<string, string> _customSettings;
        public SerializableDictionary<string, string> CustomSettings
        {
            get { return _customSettings; }
        }

        public SettingsObject()
        {
            _customSettings = new SerializableDictionary<string, string>();
            MessagingServerPort = 8080;
            ConnectionString = "";
            DefaultHtmlReportHeader =
                @"
<style type='text/css'> 
html
{
  font-family: 'Courier New', monospace;
} 
</style>";
        }

        public void SetCustomValue(string settingName, string settingValue)
        {
            if (!CustomSettings.ContainsKey(settingName))
                CustomSettings.Add(settingName, settingValue);
            else
                CustomSettings[settingName] = settingValue;
            if (string.IsNullOrEmpty(settingValue))
                CustomSettings.Remove(settingName);
        }

        public string GetCustomValue(string settingName)
        {
            return CustomSettings.ContainsKey(settingName) ? CustomSettings[settingName] : "";
        }
    }

    public static class LocalSettings
    {
        private static SettingsObject _settingsObject;

        public static int Decimals { get { return 2; } }

        public static bool AllowMultipleClients
        {
            get { return _settingsObject.AllowMultipleClients; }
            set { _settingsObject.AllowMultipleClients = value; }
        }

        public static int MessagingServerPort
        {
            get { return _settingsObject.MessagingServerPort; }
            set { _settingsObject.MessagingServerPort = value; }
        }

        public static string MessagingServerName
        {
            get { return _settingsObject.MessagingServerName; }
            set { _settingsObject.MessagingServerName = value; }
        }

        public static string TerminalName
        {
            get { return _settingsObject.TerminalName; }
            set { _settingsObject.TerminalName = value; }
        }

        public static string ConnectionString
        {
            get { return _settingsObject.ConnectionString; }
            set { _settingsObject.ConnectionString = value; }
        }

        public static bool StartMessagingClient
        {
            get { return _settingsObject.StartMessagingClient; }
            set { _settingsObject.StartMessagingClient = value; }
        }

        public static string LogoPath
        {
            get { return _settingsObject.LogoPath; }
            set { _settingsObject.LogoPath = value; }
        }

        public static string PrintFontFamily
        {
            get
            {
                if (string.IsNullOrEmpty(_settingsObject.PrintFontFamily) || _settingsObject.PrintFontFamily == "")
                {
                    _settingsObject.PrintFontFamily = "Courier New";
                    SaveSettings();
                    //return "Consolas";
                }
                return _settingsObject.PrintFontFamily;
            }
            set
            {
                _settingsObject.PrintFontFamily = value;
                SaveSettings();
            }
        }

        public static string ApiHost
        {
            get { return _settingsObject.ApiHost; }
            set { _settingsObject.ApiHost = value; SaveSettings(); }
        }

        public static TimeSpan TokenLifeTime
        {
            get { return _settingsObject.TokenLifeTime; }
            set { _settingsObject.TokenLifeTime = value; SaveSettings(); }
        }

        public static string ApiPort
        {
            get { return _settingsObject.ApiPort; }
            set { _settingsObject.ApiPort = value; SaveSettings(); }
        }

        public static string DefaultHtmlReportHeader
        {
            get { return _settingsObject.DefaultHtmlReportHeader; }
            set { _settingsObject.DefaultHtmlReportHeader = value; }
        }

        private static CultureInfo _cultureInfo;
        public static string CurrentLanguage
        {
            get { return _settingsObject.CurrentLanguage; }
            set
            {
                _settingsObject.CurrentLanguage = value;
                _cultureInfo = CultureInfo.GetCultureInfo(value);
                UpdateThreadLanguage();
                SaveSettings();
            }
        }

        public static bool OverrideWindowsRegionalSettings
        {
            get { return _settingsObject.OverrideWindowsRegionalSettings; }
            set { _settingsObject.OverrideWindowsRegionalSettings = value; }
        }

        public static int DefaultRecordLimit
        {
            get { return _settingsObject.DefaultRecordLimit; }
            set { _settingsObject.DefaultRecordLimit = value; }
        }

        public static double WindowScale
        {
            get { return _settingsObject.WindowScale; }
            set { _settingsObject.WindowScale = value; }
        }

        public static string AppPath { get; set; }
        public static string DocumentPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + AppName; } }

        public static string DataPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Ozgu Tech\\" + AppName; } }
        public static string UserPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ozgu Tech\\" + AppName; } }

        public static string CommonSettingsFileName { get { return DataPath + "\\SambaSettings.txt"; } }
        public static string UserSettingsFileName { get { return UserPath + "\\SambaSettings.txt"; } }

        public static string SettingsFileName { get { return File.Exists(UserSettingsFileName) ? UserSettingsFileName : CommonSettingsFileName; } }

        public static string CurrencyFormat { get; set; }
        public static string QuantityFormat { get; set; }
        public static string ReportCurrencyFormat { get; set; }
        public static string ReportQuantityFormat { get; set; }
        public static string PrintoutCurrencyFormat { get; set; }
        public static string CurrencySymbol { get { return CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol; } }

        private static int DefaultDbVersion { get { return 20; } }
        private static string DefaultAppVersion { get { return "3.0.26 BETA"; } }

        public static int DbVersion { get { return CanReadVersionFromFile() ? Convert.ToInt32(GetVersionDat("DbVersion")) : DefaultDbVersion; } }
        public static string AppVersion { get { return CanReadVersionFromFile() ? GetVersionDat("AppVersion") : DefaultAppVersion; } }
        public static DateTime AppVersionDateTime
        {
            get
            {
                if (!CanReadVersionFromFile()) return DateTime.Now;

                //2013-06-19 1415
                var reg = new Regex(@"(\d\d\d\d)-(\d\d)-(\d\d) (\d\d)(\d\d)");
                var match = reg.Match(GetVersionDat("VersionTime"));

                return new DateTime(Convert.ToInt32(match.Groups[1].Value),
                                    Convert.ToInt32(match.Groups[2].Value),
                                    Convert.ToInt32(match.Groups[3].Value),
                                    Convert.ToInt32(match.Groups[4].Value),
                                    Convert.ToInt32(match.Groups[5].Value),
                                    0);
            }
        }

        public static string AppName { get { return "SambaPOS3"; } }

        private static IList<string> _supportedLanguages;
        public static IList<string> SupportedLanguages { get { return _supportedLanguages ?? (_supportedLanguages = new[] { "en", "tr", "it", "pt-BR", "hr", "ar", "hu", "es", "id", "el", "zh-CN", "de", "sq", "cs", "nl", "he", "fr", "ru-RU" }); } }

        public static long CurrentDbVersion { get; set; }

        public static string DatabaseLabel
        {
            get
            {
                if (ConnectionString.ToLower().Contains(".sdf")) return "CE";
                if (ConnectionString.ToLower().Contains("data source")) return "SQ";
                if (ConnectionString.ToLower().StartsWith("mongodb://")) return "MG";
                if (string.IsNullOrEmpty(ConnectionString) && IsSqlce40Installed()) return "CE";
                return "TX";
            }
        }

        private static Dictionary<string, string> _versionData;
        private static readonly string VersionDataFilePath = DataPath + @"\version.dat";

        private static bool CanReadVersionFromFile()
        {
#if DEBUG
            return false;
#else
            return File.Exists(VersionDataFilePath);
#endif
        }

        private static string GetVersionDat(string versionType)
        {
            if (_versionData == null || _versionData.Count == 0)
            {
                _versionData = new Dictionary<string, string>();
                foreach (string item in File.ReadAllLines(VersionDataFilePath))
                {
                    string[] split = item.Split('=');
                    _versionData.Add(split[0], split[1]);
                }
            }
            if (_versionData.ContainsKey(versionType))
            {
                return _versionData[versionType];
            }
            else
            {
                throw new ArgumentOutOfRangeException("versionType", "VersionType " + versionType + " doesn't exist!");
            }
        }

        public static bool IsSqlce40Installed()
        {
            var rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server Compact Edition\\v4.0");
            return rk != null;
        }

        public static void SaveSettings()
        {
            try
            {
                var serializer = new XmlSerializer(_settingsObject.GetType());
                var writer = new XmlTextWriter(SettingsFileName, null);
                try
                {
                    serializer.Serialize(writer, _settingsObject);
                }
                finally
                {
                    writer.Close();
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (!File.Exists(UserSettingsFileName))
                {
                    File.Create(UserSettingsFileName).Close();
                    SaveSettings();
                }
            }
            catch (IOException)
            {
                return;
            }
        }

        public static void LoadSettings()
        {
            _settingsObject = new SettingsObject();
            string fileName = SettingsFileName;
            if (File.Exists(fileName))
            {
                var serializer = new XmlSerializer(_settingsObject.GetType());
                var reader = new XmlTextReader(fileName);
                try
                {
                    _settingsObject = serializer.Deserialize(reader) as SettingsObject;
                }
                finally
                {
                    reader.Close();
                }
            }
            if (DefaultRecordLimit == 0)
                DefaultRecordLimit = 100;
        }

        static LocalSettings()
        {
            if (!Directory.Exists(DocumentPath))
                Directory.CreateDirectory(DocumentPath);
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(UserPath))
                Directory.CreateDirectory(UserPath);
            LoadSettings();
        }

        public static void UpdateThreadLanguage()
        {
            if (_cultureInfo != null)
            {
                if (OverrideWindowsRegionalSettings)
                    Thread.CurrentThread.CurrentCulture = _cultureInfo;
                Thread.CurrentThread.CurrentUICulture = _cultureInfo;
            }
        }

        public static void UpdateSetting(string settingName, string settingValue)
        {
            _settingsObject.SetCustomValue(settingName, settingValue);
            SaveSettings();
        }

        public static string ReadSetting(string settingName)
        {
            return _settingsObject.GetCustomValue(settingName);
        }
    }
}
