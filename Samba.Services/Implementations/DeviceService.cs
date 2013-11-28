using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Services.Common;

namespace Samba.Services.Implementations
{
    [Export(typeof(IDeviceService))]
    class DeviceService : IDeviceService
    {
        [ImportMany]
        public IEnumerable<IDevice> Devices { get; set; }

        public IEnumerable<string> GetDeviceNames(DeviceType deviceType)
        {
            return Devices.Where(x => x.DeviceType == deviceType).Select(x => x.Name);
        }
        public IEnumerable<string> GetDeviceNames()
        {
            return Devices.Select(x => x.Name);
        }

        public void InitializeDevice(string deviceName)
        {
            if (Devices.Any(x => x.Name == deviceName))
                Devices.Single(x => x.Name == deviceName).InitializeDevice();
        }

        public void FinalizeDevices()
        {
            Devices.ToList().ForEach(x => x.FinalizeDevice());
        }

        public IDevice GetDeviceByName(string deviceName)
        {
            return Devices.FirstOrDefault(x => x.Name == deviceName);
        }
    }
}
