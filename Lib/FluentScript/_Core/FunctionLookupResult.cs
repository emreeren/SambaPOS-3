namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// The result of searching for a function that is either internal/external
    /// </summary>
    public class FunctionLookupResult
    {
        /// <summary>
        /// Null/Empty object.
        /// </summary>
        public static readonly FunctionLookupResult False = new FunctionLookupResult(false, string.Empty, MemberMode.FunctionScript);


        /// <summary>
        /// Whether or not the function represented by Name exists.
        /// </summary>
        public bool Exists;


        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name;


        /// <summary>
        /// Whether this is a internal script level or external function name.
        /// </summary>
        public MemberMode FunctionMode;


        /// <summary>
        /// The number of tokens that represented this.
        /// </summary>
        public int TokenCount;


        /// <summary>
        /// Intialize with defaults.
        /// </summary>
        public FunctionLookupResult()
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="exists"></param>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        public FunctionLookupResult(bool exists, string name, MemberMode mode)
        {
            Exists = exists;
            Name = name;
            FunctionMode = mode;
        }
    }
}
