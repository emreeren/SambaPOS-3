using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Samba.Infrastructure
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
