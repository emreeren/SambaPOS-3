using System;
using System.Linq;
using System.ServiceProcess;

namespace Samba.MessagingServer.WindowsService
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] servicesToRun;
            servicesToRun = new ServiceBase[] 
            { new MessagingServer() };
            ServiceBase.Run(servicesToRun);
        }
    }
}
