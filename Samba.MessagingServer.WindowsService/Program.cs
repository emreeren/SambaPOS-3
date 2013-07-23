using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Samba.MessagingServer.WindowsService
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (System.Environment.UserInteractive)
            {
                string parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ServiceHelper.InstallWindowsService();
                        break;
                    case "--uninstall":
                        ServiceHelper.UninstallWindowsService();
                        break;
                    case "--start":
                        ServiceHelper.StartService();
                        break;
                    case "--stop":
                        ServiceHelper.StopService();
                        break;
                }
            }
            else
        {
            ServiceBase[] servicesToRun;
            servicesToRun = new ServiceBase[] 
            { new MessagingServer() };
            ServiceBase.Run(servicesToRun);
        }
    }
    }
}