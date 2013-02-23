////////////////////////////////////////////////////////////////////////////////////
// Win32API.cs
//
// By Scott McMaster (smcmaste@hotmail.com)
// 2/1/2006
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Samba.Presentation.Controls.Browser
{
	/// <summary>
	/// Win32 api functions and definitions we use.
	/// </summary>
	public sealed class Win32API
	{
		/// <summary>
		/// From winerror.h.
		/// </summary>
		public const int ERROR_SUCCESS = 0;

		/// <summary>
		/// From winerror.h.
		/// </summary>
		public const int ERROR_FILE_NOT_FOUND = 2;

		/// <summary>
		/// From winerror.h.
		/// </summary>
		public const int ERROR_ACCESS_DENIED = 5;

		/// <summary>
		/// From winerror.h.
		/// </summary>
		public const int ERROR_INSUFFICIENT_BUFFER = 122;

		/// <summary>
		/// From winerror.h.
		/// </summary>
		public const int ERROR_NO_MORE_ITEMS = 259;

		[StructLayout(LayoutKind.Sequential)]
		public struct FILETIME
		{
			public UInt32 dwLowDateTime;
			public UInt32 dwHighDateTime;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SYSTEMTIME
		{
			public UInt16 Year;
			public UInt16 Month;
			public UInt16 DayOfWeek;
			public UInt16 Day;
			public UInt16 Hour;
			public UInt16 Minute;
			public UInt16 Second;
			public UInt16 Milliseconds;
		}

		[DllImport("Kernel32.dll", SetLastError=true)]
		public static extern long FileTimeToSystemTime(ref FILETIME FileTime, ref SYSTEMTIME SystemTime);

		[DllImport("kernel32.dll", SetLastError=true)]
		public static extern long SystemTimeToTzSpecificLocalTime(IntPtr lpTimeZoneInformation, ref SYSTEMTIME lpUniversalTime, out SYSTEMTIME lpLocalTime);

		/// <summary>
		/// Helper routine to get a DateTime from a FILETIME.
		/// </summary>
		/// <param name="ft"></param>
		/// <returns></returns>
		public static DateTime FromFileTime( Win32API.FILETIME ft )
		{
			if( ft.dwHighDateTime == Int32.MaxValue || (ft.dwLowDateTime == 0 && ft.dwHighDateTime == 0) )
			{
				// Not going to fit in the DateTime.  In the WinInet APIs, this is
				// what happens when there is no FILETIME attached to the cache entry.
				// We're going to use DateTime.MinValue as a marker for this case.
				return DateTime.MinValue;
			}

            Win32API.SYSTEMTIME syst = new Win32API.SYSTEMTIME();
            Win32API.SYSTEMTIME systLocal = new Win32API.SYSTEMTIME();
			if( 0 == Win32API.FileTimeToSystemTime( ref ft, ref syst ) )
			{
				throw new ApplicationException( "Error calling FileTimeToSystemTime: " + Marshal.GetLastWin32Error() );
			}
			if( 0 == Win32API.SystemTimeToTzSpecificLocalTime( IntPtr.Zero, ref syst, out systLocal ) )
			{
				throw new ApplicationException( "Error calling SystemTimeToTzSpecificLocalTime: " + Marshal.GetLastWin32Error() );
			}

			return new DateTime( systLocal.Year, systLocal.Month, systLocal.Day, systLocal.Hour, systLocal.Minute, systLocal.Second );
		}

		/// <summary>
		/// Get a string representation of the given FILETIME.
		/// </summary>
		/// <param name="ft"></param>
		/// <returns></returns>
		public static string ToStringFromFileTime( Win32API.FILETIME ft )
		{
			DateTime dt = FromFileTime( ft );
			if( dt == DateTime.MinValue )
			{
				return "";
			}

			return dt.ToString();
		}

		/// <summary>
		/// Static class -- can't create.
		/// </summary>
		private Win32API()
		{
		}
	}
}
