using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Class to store information about an error.
    /// </summary>
    public class ScriptError
    {
        /// <summary>
        /// File name of the script
        /// </summary>
	    public string File;


        /// <summary>
        /// The plugin that caused the error ( if applicable )
        /// </summary>
	    public string Plugin;


        /// <summary>
        /// The Line number of the error
        /// </summary>
	    public int Line;


        /// <summary>
        /// The column number of the error.
        /// </summary>
	    public int Column;


        /// <summary>
        /// The message of the error
        /// </summary>
        public string Message;


        /// <summary>
        /// The type of the error. syntax/limit.
        /// </summary>
        public string ErrorType;
    }
}
