namespace Samba.Modules.CidMonitor
{
    class AbstractCidSettings
    {
        public AbstractCidSettings()
        {
            TrimChars = "+90";
        }
        public string TrimChars { get; set; }
    }
}