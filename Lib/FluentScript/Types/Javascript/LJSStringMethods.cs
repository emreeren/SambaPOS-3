using System.Text;
using System.Text.RegularExpressions;

namespace Fluentscript.Lib.Types.Javascript
{

    /// <summary>
    /// Methods on the string datatype.
    /// </summary>
    public class LJSStringMethods : LTypeMethods
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LJSStringMethods()
        {
            DataType = LTypes.String;

            // Create the methods associated with this type.
            AddMethod("charAt",       "CharAt",       typeof(string),  "Returns the character at the specified index" );
            AddMethod("charCodeAt",   "CharAt",       typeof(string),  "Returns the Unicode of the character at the specified index" );
            AddMethod("concat",       "Concat",       typeof(string),  "Joins two or more strings, and returns a copy of the joined strings" );
            AddMethod("fromCharCode", "CharAt",       typeof(string),  "Converts Unicode values to characters" );
            AddMethod("indexOf",      "IndexOf",      typeof(double),  "Returns the position of the first found occurrence of a specified value in a string" );
            AddMethod("lastIndexOf",  "LastIndexOf",  typeof(double),  "Returns the position of the last found occurrence of a specified value in a string");
            AddMethod("match",        "CharAt",       typeof(string),  "Searches for a match between a regular expression and a string, and returns the matches" );
            AddMethod("replace",      "Replace",      typeof(string),  "Searches for a match between a substring (or regular expression) and a string, and replaces the matched substring with a new substring" );
            AddMethod("search",       "Search",       typeof(string),  "Searches for a match between a regular expression and a string, and returns the position of the match" );
            AddMethod("slice",        "CharAt",       typeof(string),  "Extracts a part of a string and returns a new string" );
            AddMethod("split",        "CharAt",       typeof(string),  "Splits a string into an array of substrings" );
            AddMethod("substr",       "Substr",       typeof(string),  "Extracts the characters from a string, beginning at a specified start position, and through the specified number of character" );
            AddMethod("substring",    "Substring",    typeof(string),  "Extracts the characters from a string, between two specified indices" );
            AddMethod("toLowerCase",  "ToLowerCase",  typeof(string),  "Converts a string to lowercase letters" );
            AddMethod("toUpperCase",  "ToUpperCase",  typeof(string),  "Converts a string to uppercase letters" );
            AddMethod("valueOf",      "ToString",     typeof(string),  "Returns the primitive value of a String object" );
            AddProperty(true, false,  "length",     "Length",       typeof(double),  "Returns the length of the string");

            // Associate the arguments for each declared function.
            //     Method name,    Param name,     Type,     Required   Alias,  Default,    Example         Description
            AddArg("charAt",       "index",        "int",    true,      "",     0,          "0 | 4",        "An integer representing the index of the character you want to return");
            AddArg("concat", 		"items",       "params", true,      "",     null,       "'abc', 'def'", "The strings to be joined");
            AddArg("indexOf", 		"pattern",     "string", true,      "",     null,       "abc",          "The string pattern to search for");
            AddArg("indexOf", 		"start",       "int",    false,     "",     0,          "0 | 5",        "The starting position of the search");
            AddArg("lastIndexOf", 	"searchvalue", "string", true,      "",     null,       "abc",          "The string to search for");
            AddArg("lastIndexOf", 	"start",       "number", false,     "",     -1,         "0 | 4",        "The position where to start the search. If omitted, the default value is the length of the string");
            AddArg("replace", 		"searchvalue", "string", true,      "",     "",         "abc",          "The value, or regular expression, that will be replaced by the new value");
            AddArg("replace", 		"newvalue",    "string", true,      "",     "",         "bbb",          "The value to replace the searchvalue with");
            AddArg("search", 		"searchvalue", "string", true,      "",     "",         "abc",          "The value, or regular expression, to search for.");
            AddArg("substr", 		"start",	   "int",    true,      "",     0,          "0 | 4",        "The postition where to start the extraction. First character is at index 0");
            AddArg("substr", 		"length",      "int",    false,     "",     "",         "5 | 10",       "The number of characters to extract. If omitted, it extracts the rest of the string" );
            AddArg("substring", 	"from",	       "int",    true,      "",     0,          "0 | 4",        "The index where to start the extraction. First character is at index 0");
            AddArg("substring", 	"to",          "int",    false,     "",     "",         "5 | 10",       "The index where to stop the extraction. If omitted, it extracts the rest of the string");
        }


        #region Javascript API methods
        /// <summary>
        /// Can create from the paramelist expressions supplied.
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <returns></returns>
        public override bool CanCreateFromArgs(object[] args)
        {
            var paramCount = args == null ? 0 : args.Length;
            if (paramCount == 0 || paramCount == 1)
                return true;
            return false;
        }


        /// <summary>
        /// Returns the character at the specified index
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="ndx">The index of the character to get</param>
        /// <returns></returns>
        public string CharAt(LString target, int ndx)
        {
            if (ndx < 0) return string.Empty;
            if (ndx >= target.Value.Length) return string.Empty;
            return target.Value[ndx].ToString();
        }


        /// <summary>
        /// Joins two or more strings, and returns a copy of the joined strings
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="strings">The list of strings to join</param>
        /// <returns></returns>
        public string Concat(LString target, object[] strings)
        {
            var result = new StringBuilder();
            result.Append(target.Value);
            foreach (object str in strings)
                result.Append(str);
            return result.ToString();
        }


        /// <summary>
        /// Returns the position of the first found occurrence of a specified value in a string
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="searchString">The string to search for</param>
        /// <param name="start">The starting position to start the search.</param>
        /// <returns></returns>
        public int IndexOf(LString target, string searchString, int start = 0)
        {
            if (string.IsNullOrEmpty(target.Value)) return -1;
            if (string.IsNullOrEmpty(searchString)) return -1;
            return target.Value.IndexOf(searchString, start);
        }


        /// <summary>
        /// Returns the position of the last found occurrence of a specified value in a string
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="searchString">The text to search for</param>
        /// <param name="start">The position to start search</param>
        /// <returns></returns>
        public int LastIndexOf(LString target, string searchString, int start)
        {
            if (string.IsNullOrEmpty(target.Value)) return -1;
            if (string.IsNullOrEmpty(searchString)) return -1;
            if (start == -1) return target.Value.LastIndexOf(searchString);

            var result = target.Value.LastIndexOf(searchString, start);
            return result;
        }


        /// <summary>
        /// Extracts the characters from a string, beginning at a specified start position, and through the specified number of character
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="from">Index where to start extraction</param>
        /// <param name="length">The number of characters to extract. If omitted, it extracts the rest of the string</param>
        /// <returns></returns>
        public string Substr(LString target, int from, int length = -1)
        {
            if (from < 0) from = 0;

            // Upto end of string.
            if (length == -1) return target.Value.Substring(from);

            return target.Value.Substring(from, length);
        }
               
        
        /// <summary>
        /// Extracts the characters from a string, between two specified indices
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="from">Index where to start extraction</param>
        /// <param name="to">The index where to stop the extraction. If omitted, it extracts the rest of the string</param>
        /// <returns></returns>
        public string Substring(LString target, int from, int to = -1)
        {
            if (from < 0) from = 0; 

            // Upto end of string.
            if (to == -1) return target.Value.Substring(from);

            // Compute length for c# string method.
            int length = (to - from) + 1;
            return target.Value.Substring(from, length);
        }

        
        /// <summary>
        /// Searches for a match between a substring (or regular expression) and a string, and replaces the matched substring with a new substring
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="substring">Required. A substring or a regular expression.</param>
        /// <param name="newString">Required. The string to replace the found value in parameter 1</param>
        /// <returns></returns>
        public string Replace(LString target, string substring, string newString)
        {
            return target.Value.Replace(substring, newString);
        }        

        
        /// <summary>
        /// Searches for a match between a regular expression and a string, and returns the position of the match
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <param name="regExp">Required. A regular expression.</param>
        /// <returns></returns>
        public int Search(LString target, string regExp)
        {
            Match match = Regex.Match(target.Value, regExp);
            if (!match.Success) return -1;

            return match.Index;
        }


        /// <summary>
        /// Converts a string to uppercase letters
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <returns></returns>
        public string ToUpperCase(LString target)
        {
            return target.Value.ToUpper();
        }


        /// <summary>
        /// Converts a string to lowercase letters
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <returns></returns>
        public string ToLowerCase(LString target)
        {
            return target.Value.ToLower();
        }


        /// <summary>
        /// Gets the length of the string
        /// </summary>
        /// <param name="target">The target value to apply this method on</param>
        /// <returns></returns>
        public int Length(LString target)
        {
            return target.Value.Length;
        }
        #endregion
    }
}
