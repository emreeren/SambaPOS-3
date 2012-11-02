using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Helper functions at the core level.
    /// </summary>
    public static class LangUtils
    {
        /// <summary>
        /// Whether or not the list supplied is null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return true;
            return false;
        }
    }
}
