using System.Collections.Generic;
using Fluentscript.Lib.Parser.Core;
// <lang:using>

// </lang:using>


namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// The results of a preprocess conditions
    /// </summary>
    public class PreprocessResults
    {
        public int Count;
        public List<string> Keys;
        public bool IsValid;
        public string Message;
        public bool IsTrue;
        public bool IsAnd;
    }



    /// <summary>
    /// Helper class to parse a preprocessor directive.
    /// </summary>
    public class PreprocessHelper
    {
        public static Context Ctx;


        /// <summary>
        /// Process the line of code for preprocessor conditions.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static PreprocessResults Process(string code)
        {
            var pos = 0;
            var end = code.Length;
            var lastword = string.Empty;

            var result = new PreprocessResults();
            result.Keys = new List<string>();
            result.IsValid = true;

            // Check 1: empty ?
            if (string.IsNullOrEmpty(code))
            {
                result.IsValid = false;
                result.Message = "Preprocessor directive is empty";
                return result;
            }

            while (pos < end)
            {
                var ch = code[pos];
                if (char.IsLetter(ch))
                {
                    lastword += ch;
                }
                else if (ch == ' ' || ch == '\t')
                {
                    if (lastword != string.Empty)
                    {
                        if (lastword == "or")
                        {
                            result.IsAnd = false;
                        }
                        else if (lastword == "and")
                        {
                            result.IsAnd = true;
                        }
                        else
                        {
                            result.Keys.Add(lastword);
                        }

                        lastword = string.Empty;
                    }
                }
                pos++;
            }

            // Last one
            if (!string.IsNullOrEmpty(lastword))
                result.Keys.Add(lastword);

            result.Count = result.Keys.Count;

            // Check 2: any keys found?
            if (result.Keys.Count == 0)
            {
                result.IsValid = false;
                result.Message = "Preprocessor directive does not have any constants";
                return result;
            }

            // Check 3: are keys present
            if (result.IsAnd)
            {
                result.IsTrue = true;
                foreach (var key in result.Keys)
                {
                    if (!Ctx.Directives.Contains(key))
                    {
                        result.IsTrue = false;
                        break;
                    }
                }
            }
            else
            {
                foreach (var key in result.Keys)
                {
                    if (Ctx.Directives.Contains(key))
                    {
                        result.IsTrue = true;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
