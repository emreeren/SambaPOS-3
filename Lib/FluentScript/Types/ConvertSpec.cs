using System;
using System.Collections.Generic;

namespace Fluentscript.Lib.Types
{
    /// <summary>
    /// Internal class for specifying what conversions can take place.
    /// </summary>
    public class ConvertSpec
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="sourceType">The source type</param>
        /// <param name="destType">The destination type</param>
        /// <param name="canChange">Whether or not the conversion change can occur with these types</param>
        /// <param name="isCaseSensitive">Whether or not the conversion is case sensitive.</param>
        /// <param name="convertMode">The mode of conversion.</param>
        /// <param name="regex">A regex pattern if mode involves regular expression checks.</param>
        /// <param name="allowedVals">A list of allowed values of source value</param>
        /// <param name="handler">A method to perform the conversion for complex conversions.</param>
        public ConvertSpec(string sourceType, string destType, bool canChange, bool isCaseSensitive,
                           int convertMode, string regex, Func<ConvertSpec, object, object> handler, List<string> allowedVals)
        {
            SourceType = sourceType;
            DestType = destType;
            CanChange = canChange;
            IsCaseSensitive = isCaseSensitive;
            ConvertMode = convertMode;
            RegexPattern = regex;
            Handler = handler;
            AllowedVals = allowedVals;
        }


        /// <summary>
        /// Source type. e.g. "string"
        /// </summary>
        public string SourceType { get; set; }


        /// <summary>
        /// Destination type e.g. "bool"
        /// </summary>
        public string DestType { get; set; }


        /// <summary>
        /// Whether or not a change can take place.
        /// </summary>
        public bool CanChange { get; set; }


        /// <summary>
        /// Whether or not the change is case sensitive
        /// </summary>
        public bool IsCaseSensitive { get; set; }


        /// <summary>
        /// The conversion mode from "direct", "regex", "handler", "list"
        /// </summary>
        public int ConvertMode { get; set; }


        /// <summary>
        /// The regex pattern to use for conversion checking.
        /// </summary>
        public string RegexPattern { get; set; }


        /// <summary>
        /// The list of allowed source values
        /// </summary>
        public List<string> AllowedVals { get; set; }


        /// <summary>
        /// A method handler for more complex conversions.
        /// </summary>
        public Func<ConvertSpec, object, object> Handler { get; set; }


        /// <summary>
        /// Lookup key
        /// </summary>
        /// <returns></returns>
        public string LookupKey()
        {
            return this.SourceType + "-" + this.DestType;
        }
    }
}
