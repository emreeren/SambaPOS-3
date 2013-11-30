using System.ComponentModel;
using System.IO.Ports;
using PropertyTools.DataAnnotations;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.CidMonitor
{
    class GenericModemSettings : AbstractCidSettings
    {
        private NameWithValue _portNameValue;

        public GenericModemSettings()
        {
            PortNameValue.UpdateValues(SerialPort.GetPortNames());
        }

        [DisplayName("Port")]
        public NameWithValue PortNameValue
        {
            get { return _portNameValue ?? (_portNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string PortName { get { return PortNameValue.Text; } set { PortNameValue.Text = value; } }
        [WideProperty]
        [Height(80)]
        public string InitializationString { get; set; }
        public string MatchPattern { get; set; }
        public string TerminateString { get; set; }
    }
}