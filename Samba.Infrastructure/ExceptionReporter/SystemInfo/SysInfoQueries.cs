namespace Samba.Infrastructure.ExceptionReporter.SystemInfo
{
	internal static class SysInfoQueries
    {
        public static readonly SysInfoQuery OperatingSystem = new SysInfoQuery("Operating System", "Win32_OperatingSystem", false);
        public static readonly SysInfoQuery Machine = new SysInfoQuery("Machine", "Win32_ComputerSystem", true);
    }
}