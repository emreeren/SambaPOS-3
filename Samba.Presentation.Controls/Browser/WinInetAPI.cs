////////////////////////////////////////////////////////////////////////////////////
// WinInetAPI.cs
//
// By Scott McMaster (smcmaste@hotmail.com)
// 2/1/2006
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Samba.Presentation.Controls.Browser
{
    /// <summary>
    /// Interop functions we need (i.e. those dealing with the url cache) from wininet.dll.
    /// </summary>
    public sealed class WinInetAPI
    {
        /// <summary>
        /// Structure used in various caching APIs.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct INTERNET_CACHE_ENTRY_INFO
        {
            public UInt32 dwStructSize;
            public string lpszSourceUrlName;
            public string lpszLocalFileName;
            public UInt32 CacheEntryType;
            public UInt32 dwUseCount;
            public UInt32 dwHitRate;
            public UInt32 dwSizeLow;
            public UInt32 dwSizeHigh;
            public Win32API.FILETIME LastModifiedTime;
            public Win32API.FILETIME ExpireTime;
            public Win32API.FILETIME LastAccessTime;
            public Win32API.FILETIME LastSyncTime;
            public IntPtr lpHeaderInfo;
            public UInt32 dwHeaderInfoSize;
            public string lpszFileExtension;
            public UInt32 dwExemptDelta;
        };

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern long FindCloseUrlCache(IntPtr hEnumHandle);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern IntPtr FindFirstUrlCacheEntry(string lpszUrlSearchPattern, IntPtr lpFirstCacheEntryInfo, out UInt32 lpdwFirstCacheEntryInfoBufferSize);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern long FindNextUrlCacheEntry(IntPtr hEnumHandle, IntPtr lpNextCacheEntryInfo, out UInt32 lpdwNextCacheEntryInfoBufferSize);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool GetUrlCacheEntryInfo(string lpszUrlName, IntPtr lpCacheEntryInfo, out UInt32 lpdwCacheEntryInfoBufferSize);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern long DeleteUrlCacheEntry(string lpszUrlName);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern IntPtr RetrieveUrlCacheEntryStream(string lpszUrlName, IntPtr lpCacheEntryInfo, out UInt32 lpdwCacheEntryInfoBufferSize, long fRandomRead, UInt32 dwReserved);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern IntPtr ReadUrlCacheEntryStream(IntPtr hUrlCacheStream, UInt32 dwLocation, IntPtr lpBuffer, out UInt32 lpdwLen, UInt32 dwReserved);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern long UnlockUrlCacheEntryStream(IntPtr hUrlCacheStream, UInt32 dwReserved);

        /// <summary>
        /// Remove an entry from the url cache.
        /// </summary>
        /// <param name="url"></param>
        public static void DeleteFromUrlCache(string url)
        {
            long apiResult = DeleteUrlCacheEntry(url);
            if (apiResult != 0)
            {
                return;
            }

            int lastError = Marshal.GetLastWin32Error();
            if (lastError == Win32API.ERROR_ACCESS_DENIED)
            {
                ThrowAccessDenied(url);
            }
            else
            {
                ThrowFileNotFound(url);
            }
        }

        /// <summary>
        /// Helper method to throw a standard access denied exception.
        /// </summary>
        private static void ThrowAccessDenied(string url)
        {
            throw new ApplicationException("Access denied: " + url);
        }

        /// <summary>
        /// Helper method to throw a standard insufficient buffer exception.
        /// </summary>
        private static void ThrowInsufficientBuffer(string url)
        {
            throw new ApplicationException("Insufficient buffer: " + url);
        }

        /// <summary>
        /// Helper method to throw a standard file not found exception.
        /// </summary>
        private static void ThrowFileNotFound(string url)
        {
            throw new ApplicationException("File not found: " + url);
        }

        /// <summary>
        /// Helper method to check for standard errors we may see from the WinInet functions.
        /// </summary>
        /// <param name="url"></param>
        private static void CheckLastError(string url, bool ignoreInsufficientBuffer)
        {
            int lastError = Marshal.GetLastWin32Error();
            if (lastError == Win32API.ERROR_INSUFFICIENT_BUFFER)
            {
                if (!ignoreInsufficientBuffer)
                {
                    ThrowInsufficientBuffer(url);
                }
            }
            else if (lastError == Win32API.ERROR_FILE_NOT_FOUND)
            {
                ThrowFileNotFound(url);
            }
            else if (lastError == Win32API.ERROR_ACCESS_DENIED)
            {
                ThrowAccessDenied(url);
            }
            else if (lastError != Win32API.ERROR_SUCCESS)
            {
                throw new ApplicationException("Unexpected error, code=" + lastError.ToString());
            }
        }

        /// <summary>
        /// More friendly wrapper for the GetUrlCacheEntryInfo API.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static INTERNET_CACHE_ENTRY_INFO GetUrlCacheEntryInfo(string url)
        {
            Uri u = new Uri(url);
            url = u.AbsoluteUri;
            IntPtr buffer = IntPtr.Zero;
            UInt32 structSize;
            bool apiResult = GetUrlCacheEntryInfo(url, buffer, out structSize);
            CheckLastError(url, true);

            try
            {
                buffer = Marshal.AllocHGlobal((int)structSize);
                apiResult = GetUrlCacheEntryInfo(url, buffer, out structSize);
                if (apiResult == true)
                {
                    return (INTERNET_CACHE_ENTRY_INFO)Marshal.PtrToStructure(buffer, typeof(INTERNET_CACHE_ENTRY_INFO));
                }

                CheckLastError(url, false);
            }
            finally
            {
                if (buffer.ToInt32() > 0)
                {
                    try { Marshal.FreeHGlobal(buffer); }
                    catch { }
                }
            }

            Debug.Assert(false, "We should either early-return or throw before we get here");
            return new INTERNET_CACHE_ENTRY_INFO();		// Make the compiler happy even though we never expect this code to run.
        }

        /// <summary>
        /// More friendly wrapper for the Retrieve/ReadUrlCacheEntryStream APIs.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string RetrieveUrlCacheEntryContents(string url)
        {
            IntPtr buffer = IntPtr.Zero;
            INTERNET_CACHE_ENTRY_INFO info = new INTERNET_CACHE_ENTRY_INFO();
            UInt32 structSize;
            IntPtr hStream = IntPtr.Zero;

            RetrieveUrlCacheEntryStream(url, buffer, out structSize, 0, 0);
            CheckLastError(url, true);
            try
            {
                buffer = Marshal.AllocHGlobal((int)structSize);
                hStream = RetrieveUrlCacheEntryStream(url, buffer, out structSize, 0, 0);
                CheckLastError(url, true);

                info = (INTERNET_CACHE_ENTRY_INFO)Marshal.PtrToStructure(buffer, typeof(INTERNET_CACHE_ENTRY_INFO));
                uint streamSize = info.dwSizeLow;
                IntPtr outBuffer = Marshal.AllocHGlobal((int)streamSize);
                try
                {
                    IntPtr result = ReadUrlCacheEntryStream(hStream, 0, outBuffer, out streamSize, 0);
                    CheckLastError(url, false);
                    return Marshal.PtrToStringAnsi(outBuffer);
                }
                finally
                {
                    if (outBuffer.ToInt32() > 0)
                    {
                        try { Marshal.FreeHGlobal(outBuffer); }
                        catch { }
                    }
                }
            }
            finally
            {
                if (buffer.ToInt32() > 0)
                {
                    try { Marshal.FreeHGlobal(buffer); }
                    catch { }
                }
                if (hStream != IntPtr.Zero)
                {
                    UInt32 dwReserved = 0;
                    UnlockUrlCacheEntryStream(hStream, dwReserved);
                }
            }
        }

        /// <summary>
        /// Friendly wrapper around the FindUrlCacheEntry APIs that gets a list of the entries
        /// matching the given pattern.
        /// </summary>
        /// <param name="urlPattern">The pattern, which is a regular expression applied by this method, since I've never
        /// seen any evidence that the first parameter to the FindFirstUrlCacheEntry API actually works.</param>
        /// <returns></returns>
        public static ArrayList FindUrlCacheEntries(string urlPattern)
        {
            ArrayList results = new ArrayList();

            IntPtr buffer = IntPtr.Zero;
            UInt32 structSize;
            IntPtr hEnum = FindFirstUrlCacheEntry(null, buffer, out structSize);
            try
            {
                if (hEnum == IntPtr.Zero)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    if (lastError == Win32API.ERROR_INSUFFICIENT_BUFFER)
                    {
                        buffer = Marshal.AllocHGlobal((int)structSize);
                        hEnum = FindFirstUrlCacheEntry(urlPattern, buffer, out structSize);
                    }
                    else if (lastError == Win32API.ERROR_NO_MORE_ITEMS)
                    {
                        return results;
                    }
                }

                INTERNET_CACHE_ENTRY_INFO result = (INTERNET_CACHE_ENTRY_INFO)Marshal.PtrToStructure(buffer, typeof(INTERNET_CACHE_ENTRY_INFO));
                try
                {
                    if (Regex.IsMatch(result.lpszSourceUrlName, urlPattern, RegexOptions.IgnoreCase))
                    {
                        results.Add(result);
                    }
                }
                catch (ArgumentException ae)
                {
                    throw new ApplicationException("Invalid regular expression, details=" + ae.Message);
                }

                if (buffer != IntPtr.Zero)
                {
                    try { Marshal.FreeHGlobal(buffer); }
                    catch { }
                    buffer = IntPtr.Zero;
                    structSize = 0;
                }

                while (true)
                {
                    long nextResult = FindNextUrlCacheEntry(hEnum, buffer, out structSize);
                    if (nextResult != 1)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        if (lastError == Win32API.ERROR_INSUFFICIENT_BUFFER)
                        {
                            buffer = Marshal.AllocHGlobal((int)structSize);
                            nextResult = FindNextUrlCacheEntry(hEnum, buffer, out structSize);
                        }
                        else if (lastError == Win32API.ERROR_NO_MORE_ITEMS)
                        {
                            break;
                        }
                    }

                    result = (INTERNET_CACHE_ENTRY_INFO)Marshal.PtrToStructure(buffer, typeof(INTERNET_CACHE_ENTRY_INFO));
                    if (Regex.IsMatch(result.lpszSourceUrlName, urlPattern, RegexOptions.IgnoreCase))
                    {
                        results.Add(result);
                    }

                    if (buffer != IntPtr.Zero)
                    {
                        try { Marshal.FreeHGlobal(buffer); }
                        catch { }
                        buffer = IntPtr.Zero;
                        structSize = 0;
                    }
                }
            }
            finally
            {
                if (hEnum != IntPtr.Zero)
                {
                    FindCloseUrlCache(hEnum);
                }
                if (buffer != IntPtr.Zero)
                {
                    try { Marshal.FreeHGlobal(buffer); }
                    catch { }
                }
            }

            return results;
        }

        /// <summary>
        /// Static class -- can't create.
        /// </summary>
        private WinInetAPI()
        {
        }
    }
}
