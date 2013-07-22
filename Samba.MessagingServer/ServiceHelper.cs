using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace Samba.MessagingServer
{
    public static class ServiceHelper
    {
        private static readonly string _installAssembly = "Samba.MessagingServer.WindowsService.exe";
        private static readonly string _logFile = "WindowsServiceInstall.log";
        private static readonly string _serviceName = "SambaPOS3-MessagingServer";
        private static ServiceController _ctl;

        public static ServiceControllerStatus? CheckServiceStatus()
        {
            IniitServiceController();
            if (_ctl == null)
            { return null; }
            else
            { return _ctl.Status; }
        }
  
        public static bool InstallWindowsService()
        {
            IDictionary mySavedState = new Hashtable();           

            try
            {
                // Set the commandline argument array for 'logfile'.
                string[] commandLineOptions = new string[1] { string.Format("/LogFile={0}", _logFile) };

                // Create an object of the 'AssemblyInstaller' class.
                AssemblyInstaller myAssemblyInstaller = new
                AssemblyInstaller(_installAssembly, commandLineOptions);
                

                myAssemblyInstaller.UseNewContext = true;

                // Install the 'MyAssembly' assembly.
                myAssemblyInstaller.Install(mySavedState);

                // Commit the 'MyAssembly' assembly.
                myAssemblyInstaller.Commit(mySavedState);
            }
            catch (Exception e)
            { return false; }

            StartService();
            return true;
        }

        public static void StartService()
        {
            IniitServiceController();
            if (_ctl.Status == ServiceControllerStatus.Stopped)
            { _ctl.Start(); }
        }

        public static void StopService()
        {
            IniitServiceController();          
            if (_ctl.Status == ServiceControllerStatus.Running)
            { _ctl.Stop(); }
        }

        public static bool UninstallWindowsService()
        {
            StopService();
            IDictionary mySavedState = new Hashtable();

            try
            {
                // Set the commandline argument array for 'logfile'.
                string[] commandLineOptions = new string[1] { string.Format("/LogFile={0}", _logFile) };

                // Create an object of the 'AssemblyInstaller' class.
                AssemblyInstaller myAssemblyInstaller = new
                AssemblyInstaller(_installAssembly, commandLineOptions);


                myAssemblyInstaller.UseNewContext = true;

                // Install the 'MyAssembly' assembly.
                myAssemblyInstaller.Uninstall(mySavedState);

                // Commit the 'MyAssembly' assembly.
                myAssemblyInstaller.Commit(mySavedState);
            }
            catch (Exception e)
            { return false; }

            return true;
        }

        private static void IniitServiceController()
        { _ctl = ServiceController.GetServices()
                                  .Where(s => s.ServiceName == _serviceName)
                                  .FirstOrDefault(); }
    }
}
