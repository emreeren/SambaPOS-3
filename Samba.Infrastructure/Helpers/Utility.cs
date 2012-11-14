using System;
using System.IO;

namespace Samba.Infrastructure.Helpers
{
    public static class Utility
    {
        public static bool IsValidFile(string fileName)
        {
            fileName = fileName.Trim();
            if (fileName == "." || !fileName.Contains(".")) return false;
            var result = false;
            try
            {
                new FileInfo(fileName);
                result = true;
            }
            catch (ArgumentException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (NotSupportedException)
            {
            }
            return result;
        }
    }
}
