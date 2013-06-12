using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Services.Common;

namespace Samba.Services
{
    public interface IDeviceService
    {
        IEnumerable<string> GetDeviceNames();
        void InitializeDevices();
    }
}
