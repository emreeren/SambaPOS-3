using System.Collections.Generic;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Parser.Integration
{
    /// <summary>
    /// Contextual information for phases in the interpreter.
    /// </summary>
    public class PhaseResult : RunResult
    {

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="started"></param>
        /// <param name="ended"></param>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public PhaseResult(RunResult result)
            : base(result.StartTime, result.EndTime, result.Success, result.Message)
        {
            this.Errors = result.Errors;
            this.Result = result;
        }


        /// <summary>
        /// The run result.
        /// </summary>
        public RunResult Result;


        /// <summary>
        /// A dictionary of items for passing some data back to the result.
        /// </summary>
        public Dictionary<string, object> Items;
    }
}
