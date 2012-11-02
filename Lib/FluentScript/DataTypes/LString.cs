using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ComLib.Lang.Helpers;


namespace ComLib.Lang
{
    /// <summary>
    /// Boolean datatype.
    /// </summary>
    public class LString : LBaseType
    {
        private static Dictionary<string, Func<LString, ArgsFetcher, object>> _methods;

        /// <summary>
        /// Initialize
        /// </summary>
        static LString()
        {
            _methods = new Dictionary<string, Func<LString, ArgsFetcher, object>>();
            _methods["charAt"       ] = (str, fetcher) => str.CharAt(fetcher);
            _methods["charCodeAt"   ] = (str, fetcher) => str.CharAt(fetcher);
            _methods["concat"       ] = (str, fetcher) => str.Concat(fetcher.Args);
            _methods["fromCharCode" ] = (str, fetcher) => str.ToString();
            _methods["indexOf"      ] = (str, fetcher) => str.IndexOf(fetcher.Get<string>(0), fetcher.Get<int>(1, 0));
            _methods["lastIndexOf"  ] = (str, fetcher) => str.LastIndexOf(fetcher.Get<string>(0), fetcher.Get<int>(1, -1));
            _methods["match"        ] = (str, fetcher) => str.ToString();
            _methods["replace"      ] = (str, fetcher) => str.Replace(fetcher.Get<string>(0), fetcher.Get<string>(1));
            _methods["search"       ] = (str, fetcher) => str.Search(fetcher.Get<string>(0));
            _methods["slice"        ] = (str, fetcher) => str.ToString();
            _methods["split"        ] = (str, fetcher) => str.ToString();
            _methods["substr"       ] = (str, fetcher) => str.Substr(fetcher.Get<int>(0), fetcher.Get<int>(1, -1));
            _methods["substring"    ] = (str, fetcher) => str.Substring(fetcher.Get<int>(0), fetcher.Get<int>(1, -1));
            _methods["toLowerCase"  ] = (str, fetcher) => str.ToLowerCase();
            _methods["toUpperCase"  ] = (str, fetcher) => str.ToUpperCase();
            _methods["valueOf"      ] = (str, fetcher) => str.ToString();
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context">Context for the script</param>
        /// <param name="val">Value of the string</param>
        /// <param name="varName">Name of the variable</param>
        public LString(Context context, string varName, string val)
        {
            _context = context;
            _varName = varName;
            Raw = val;
        }


        /// <summary>
        /// Raw value
        /// </summary>
        public string Raw;


        
        /// <summary>
        /// Get string value.
        /// </summary>
        /// <returns></returns>
        public string ToStr()
        {
            return (string)Raw;
        }


        /// <summary>
        /// Whether or not this type supports the supplied method
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        public override bool HasMethod(string methodname)
        {
            return _methods.ContainsKey(methodname);
        }


        /// <summary>
        /// Whether or not this type supports the supplied property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override bool HasProperty(string propertyName)
        {
            return string.Compare(propertyName, "length", StringComparison.InvariantCultureIgnoreCase) == 0;
        }


        /// <summary>
        /// Calls the method
        /// </summary>
        /// <param name="methodname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object ExecuteMethod(string methodname, object[] args)
        {
            ArgsFetcher fetcher = new ArgsFetcher(args);
            if (methodname == "length")
            {
                return this.Raw.Length;
            }

            object result = _methods[methodname](this, fetcher);
            return result;
        }


        #region Javascript API methods
        /// <summary>
        /// Returns the character at the specified index
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string CharAt(ArgsFetcher args)
        {
            int ndx = args.Get<int>(0);
            if (ndx < 0) return string.Empty;
            if (ndx >= Raw.Length) return string.Empty;
            return Raw[ndx].ToString();
        }


        /// <summary>
        /// Joins two or more strings, and returns a copy of the joined strings
        /// </summary>
        /// <param name="strings">The list of strings to join</param>
        /// <returns></returns>
        public string Concat(params object[] strings)
        {
            var result = new StringBuilder();
            result.Append(this.Raw);
            foreach (object str in strings)
                result.Append(str);
            return result.ToString();
        }


        /// <summary>
        /// Returns the position of the first found occurrence of a specified value in a string
        /// </summary>
        /// <param name="searchString">The string to search for</param>
        /// <param name="start">The starting position to start the search.</param>
        /// <returns></returns>
        public int IndexOf(string searchString, int start = 0)
        {
            return Raw.IndexOf(searchString, start);
        }


        /// <summary>
        /// Returns the position of the last found occurrence of a specified value in a string
        /// </summary>
        /// <param name="searchString">The text to search for</param>
        /// <param name="start">The position to start search</param>
        /// <returns></returns>
        public int LastIndexOf(string searchString, int start)
        {
            if (start == -1)
                return Raw.LastIndexOf(searchString);

            return Raw.LastIndexOf(searchString, start);
        }


        /// <summary>
        /// Extracts the characters from a string, beginning at a specified start position, and through the specified number of character
        /// </summary>
        /// <param name="from">Index where to start extraction</param>
        /// <param name="length">The number of characters to extract. If omitted, it extracts the rest of the string</param>
        /// <returns></returns>
        public string Substr(int from, int length = -1)
        {
            if (from < 0) from = 0;

            // Upto end of string.
            if (length == -1) return Raw.Substring(from);

            return Raw.Substring(from, length);
        }
               
        
        /// <summary>
        /// Extracts the characters from a string, between two specified indices
        /// </summary>
        /// <param name="from">Index where to start extraction</param>
        /// <param name="to">The index where to stop the extraction. If omitted, it extracts the rest of the string</param>
        /// <returns></returns>
        public string Substring(int from, int to = -1)
        {
            if (from < 0) from = 0; 

            // Upto end of string.
            if (to == -1) return Raw.Substring(from);

            // Compute length for c# string method.
            int length = (to - from) + 1;
            return Raw.Substring(from, length);
        }

        
        /// <summary>
        /// Searches for a match between a substring (or regular expression) and a string, and replaces the matched substring with a new substring
        /// </summary>
        /// <param name="substring">Required. A substring or a regular expression.</param>
        /// <param name="newString">Required. The string to replace the found value in parameter 1</param>
        /// <returns></returns>
        public string Replace(string substring, string newString)
        {
            return Raw.Replace(substring, newString);
        }        

        
        /// <summary>
        /// Searches for a match between a regular expression and a string, and returns the position of the match
        /// </summary>
        /// <param name="regExp">Required. A regular expression.</param>
        /// <returns></returns>
        public int Search(string regExp)
        {
            Match match = Regex.Match(Raw, regExp);
            if (!match.Success) return -1;

            return match.Index;
        }


        /// <summary>
        /// Converts a string to uppercase letters
        /// </summary>
        /// <returns></returns>
        public string ToUpperCase()
        {
            return Raw.ToUpper();
        }


        /// <summary>
        /// Converts a string to lowercase letters
        /// </summary>
        /// <returns></returns>
        public string ToLowerCase()
        {
            return Raw.ToLower();
        }
        #endregion
    }
}
