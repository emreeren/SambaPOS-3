using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang.Core
{
    /// <summary>
    /// Interface for looking up functions
    /// </summary>
    public interface IFunctionLookup
    {
        /// <summary>
        /// Whether or not the function name exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Contains(string name);


        /// <summary>
        /// Gets the matching formal ( case sensitive name ) for the case insensitive name supplied.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetMatch(string name);
    }
}
