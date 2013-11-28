namespace Samba.Modules.CidMonitor
{
    class GenericModemSettings : AbstractCidSettings
    {
        public string PortName { get; set; }
        public string MatchPattern { get; set; }
    }
}