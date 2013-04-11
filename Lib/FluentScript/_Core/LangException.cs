using System;

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Exception used in script parsing
    /// </summary>
    public class LangException : Exception
    {
        /// <summary>
        /// The error info.
        /// </summary>
        public ScriptError Error;


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="errorType">Type of the error. "Syntax Error"</param>
        /// <param name="error">Error message</param>
        /// <param name="scriptpath">Path of the script</param>
        /// <param name="lineNumber">Line number of the error.</param>
        /// <param name="charPos">The char position of the error.</param>
        public LangException(string errorType, string error, string scriptpath, int lineNumber, int charPos = 0) : base(error)
        {
            Error = new ScriptError();
            Error.Line = lineNumber;
            Error.Message = error;
            Error.ErrorType = errorType;
            Error.File = scriptpath;
            Error.Column = charPos;
        }
    }


    /// <summary>
    /// Exception used in script parsing
    /// </summary>
    public class LangFailException : LangException
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="error">Error message</param>
        /// <param name="scriptpath">Script where error occurred.</param>
        /// <param name="lineNumber">Line number where error occurred.</param>
        public LangFailException(string error, string scriptpath, int lineNumber)
            : base("Exit Error", error, scriptpath, lineNumber) 
        {
        }
    }



    /// <summary>
    /// Exception used in script for sandbox/limits functionality. e.g. loop/callstack limits.
    /// </summary>
    public class LangLimitException : LangException
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="error">Error message</param>
        /// <param name="scriptpath">Script where error occurred.</param>
        /// <param name="lineNumber">Line number where error occurred.</param>
        public LangLimitException(string error, string scriptpath, int lineNumber)
            : base("Limit Error", error, scriptpath, lineNumber) {  }
    }
}
