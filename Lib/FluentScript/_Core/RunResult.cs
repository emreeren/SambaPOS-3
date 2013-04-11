using System;
using System.Collections.Generic;

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Result of a script action.
    /// </summary>
    //[Serializable]
    public class RunResult
    {
        /// <summary>
        /// The starttime of an action
        /// </summary>
        public readonly DateTime StartTime;


        /// <summary>
        /// The end time of an action.
        /// </summary>
        public readonly DateTime EndTime;


        /// <summary>
        /// Whether or not the result of the action was succcessful
        /// </summary>
        public readonly bool Success;


        /// <summary>
        /// A message representing the result of the action.
        /// </summary>
        public readonly string Message;


        /// <summary>
        /// Duration between start and endtime.
        /// </summary>
        public readonly TimeSpan Duration;


        /// <summary>
        /// A object that can be return from the result.
        /// </summary>
        public object Item;


        /// <summary>
        /// The exception from running script.
        /// </summary>
        public Exception Ex;


        /// <summary>
        /// List of all the script errors.
        /// </summary>
        public List<ScriptError> Errors;


        /// <summary>
        /// Total number of script errors.
        /// </summary>
        public int TotalErrors; 


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="started">Start time of script</param>
        /// <param name="ended">End time of script.</param>
        /// <param name="success">Whether or not the script execution was successful</param>
        /// <param name="message">A combined message of all the errors.</param>
        public RunResult(DateTime started, DateTime ended, bool success, string message)
        {
            StartTime = started;
            EndTime = ended;
            Success = success;
            Message = message;
            Duration = ended - started;
            TotalErrors = 1;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="started">Start time of script</param>
        /// <param name="ended">End time of script.</param>
        /// <param name="success">Whether or not the script execution was successful</param>
        /// <param name="errors">List of all the errors.</param>
        public RunResult(DateTime started, DateTime ended, bool success, List<ScriptError> errors)
        {
            StartTime = started;
            EndTime = ended;
            Success = success;
            Message = string.Empty;
            Duration = ended - started;
            TotalErrors = 0;
            Errors = errors;
            if (errors != null && errors.Count > 0)
            {
                TotalErrors = errors.Count;
                Message = errors[0].Message;
            }
        }


        /// <summary>
        /// string representation of run result
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string template = "Success: {0} - Duration: {1} - Start: {2} - End: {3} - Error(s): {4} - Message: {5}";
            string result = string.Format(template, Success, Duration.Milliseconds, StartTime.ToShortTimeString(), EndTime.ToShortTimeString(), TotalErrors, Message );
            return result;
        }
    }
}
