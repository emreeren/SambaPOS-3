namespace Fluentscript.Lib._Core.Meta.Docs
{
    /// <summary>
    /// An example call for a function
    /// </summary>
    public class Example
    {
        /// <summary>
        /// Used to identify one example from another example
        /// </summary>
        public string Tag { get; set; }


        /// <summary>
        /// Description for the example
        /// </summary>
        public string Desc { get; set; }


        /// <summary>
        /// The code representing the example.
        /// </summary>
        public string Code { get; set; }
    }
}
