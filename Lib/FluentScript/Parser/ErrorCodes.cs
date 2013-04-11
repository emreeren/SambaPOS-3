using System.Collections.Generic;

namespace Fluentscript.Lib.Parser
{
    /// <summary>
    /// Holds information about an error/warning etc.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="msg"></param>
        public ErrorInfo(string errorType, bool supportsParams, string msg)
        {
            this.ErrorType = errorType;
            this.Message = msg;
            this.SupportsParams = supportsParams;
        }


        public string Message;
        public string ErrorType;
        public bool SupportsParams;
        public string Format(string arg1)
        {
            return string.Format(Message, arg1);
        }

    }



    /// <summary>
    /// Class for representing error codes and their descriptions.
    /// </summary>
    public class ErrorCodes
    {
        private static Dictionary<string, ErrorInfo> _errors = new Dictionary<string, ErrorInfo>();
        private static ErrorInfo _emptyError = new ErrorInfo("empty", false, "");


        public const string Func1000 = "func1000";
        public const string Func1001 = "func1001";
        public const string Func1002 = "func1002";
        public const string Func1003 = "func1003";
        public const string Func1004 = "func1004";
        public const string Func1005 = "func1005";
        public const string Func1006 = "func1006";
        public const string Func1007 = "func1007";


        /// <summary>
        /// Initialize the errors.
        /// </summary>
        public static void Init()
        {
            _errors[ErrorCodes.Func1000] = new ErrorInfo("error", true, "Function call to non existant function {0}");
            _errors[ErrorCodes.Func1001] = new ErrorInfo("error", true, "Function {0} can not be declared at position");
            _errors[ErrorCodes.Func1002] = new ErrorInfo("error", false, "Named parameter does not exist");
            _errors[ErrorCodes.Func1003] = new ErrorInfo("error", false, "Parameter can not be named arguments");
            _errors[ErrorCodes.Func1004] = new ErrorInfo("error", false, "Number of parameters has exceeded limit");
            _errors[ErrorCodes.Func1005] = new ErrorInfo("error", false, "Number of aliases has exceeded limit");
            _errors[ErrorCodes.Func1006] = new ErrorInfo("error", false, "Parameter is not a valid type");
            _errors[ErrorCodes.Func1007] = new ErrorInfo("error", false, "Function is declared more than once");
        }


        /// <summary>
        /// Gets the error infor the supplied error code.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static ErrorInfo GetError(string errorCode)
        {
            if (!_errors.ContainsKey(errorCode))
                return _emptyError;

            return _errors[errorCode];
        }
    }
}
