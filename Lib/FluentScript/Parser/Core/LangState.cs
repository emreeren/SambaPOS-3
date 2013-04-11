using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Parser.Core
{
    /// <summary>
    /// State of the language. e.g. loop limits, recursion limits.
    /// </summary>
    public class LangState
    {
        /// <summary>
        /// Intiailize
        /// </summary>
        /// <param name="stack"></param>
        public LangState(CallStack stack)
        {
            _callStack = stack;
        }


        /// <summary>
        /// Number of statements
        /// </summary>
        public int StatementCount;


        /// <summary>
        /// Number of times a loop has been done.
        /// </summary>
        public int LoopCount;


        /// <summary>
        /// Number of times a recusive call has been made.
        /// </summary>
        public int RecursionCount;


        /// <summary>
        /// Total number of exceptions.
        /// </summary>
        public int ExceptionCount;


        /// <summary>
        /// How many times string are appended.
        /// </summary>
        public int StringAppendCount;


        private CallStack _callStack;
        /// <summary>
        /// The call stack for function calls.
        /// </summary>
        public CallStack Stack
        {
            get { return _callStack; }
        }


        /// <summary>
        /// Resets the state.
        /// </summary>
        public void Reset()
        {
            LoopCount = 0;
            RecursionCount = 0;
            StringAppendCount = 0;
            StatementCount = 0;
            ExceptionCount = 0;
        }
    }
}
