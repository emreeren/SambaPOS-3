namespace Samba.Modules.CidMonitor
{
    class AbstractCidSettings
    {
        public AbstractCidSettings()
        {
            TrimChars = "+90";
            PopupName = "";
        }

        public string PopupName { get; set; }
        public string TrimChars { get; set; }
    }
}