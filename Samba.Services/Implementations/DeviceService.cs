using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Services.Common;

namespace Samba.Services.Implementations
{
    [Export(typeof(IDeviceService))]
    class DeviceService : IDeviceService
    {
        [ImportMany]
        public IEnumerable<IDevice> Devices { get; set; }

        public IEnumerable<string> GetDeviceNames()
        {
            return Devices.Select(x => x.Name);
        }

        public void InitializeDevices()
        {
            Devices.ToList().ForEach(x => x.Initialize());
        }
    }
}
