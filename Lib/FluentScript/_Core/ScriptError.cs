namespace Fluentscript.Lib._Core
{

    /// <summary>
    /// Language validation result type. info, warn, compile error.
    /// </summary>
    public enum ScriptErrorType
    {
        /// <summary>
        /// Information
        /// </summary>
        Info,


        /// <summary>
        /// Warning
        /// </summary>
        Warning,


        /// <summary>
        /// Error
        /// </summary>
        Error
    }



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
        /// The type of the error. syntax/limit/error/warning.
        /// </summary>
        public string ErrorType;


        /// <summary>
        /// A distinct error code.
        /// </summary>
        public string ErrorCode;
    }
}
