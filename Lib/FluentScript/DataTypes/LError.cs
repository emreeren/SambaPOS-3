using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Error class for exceptions in the language.
    /// </summary>
    public class LError : LObject
    {
        /// <summary>
        /// Name of the message
        /// </summary>
        public string name { get; set; }


        /// <summary>
        /// Message
        /// </summary>
        public string message { get; set; }


        /// <summary>
        /// The source script that caused the error.
        /// </summary>
        public string Source { get; set; }


        /// <summary>
        /// Line number that caused the error.
        /// </summary>
        public int LineNumber { get; set; }


        /// <summary>
        /// Information about the stack trace.
        /// </summary>
        public string StackTrace { get; set; }


        /// <summary>
        /// Inner excpetion.
        /// </summary>
        public LangException Inner { get; set; }


        /// <summary>
        /// Converts from a LangException to LError datatype
        /// </summary>
        /// <param name="exc">The exception to convert to an LError</param>
        /// <returns></returns>
        public static LError FromException(Exception exc)
        {
            LError error = null;
            if (exc is LangException)
            {
                var ex = exc as LangException;
                // Create new instance of language error.
                error = new LError()
                {
                    LineNumber = ex.Error.Line,
                    name = ex.Error.Message,
                    message = ex.Message,
                    Source = ex.Error.File,
                    StackTrace = ex.StackTrace
                };
                return error;
            }
            // Create new instance of language error.
            error = new LError()
            {
                name = string.Empty,
                message = exc.Message,
                StackTrace = exc.StackTrace
            };
            return error;
        }
    }
}
