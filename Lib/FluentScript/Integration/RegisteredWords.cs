using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Helper class for calling functions
    /// </summary>
    public class RegisteredWords : Dictionary<string, string>
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public RegisteredWords()
        {
        }


        /// <summary>
        /// Register a custom type into the interpreter( for calling c# from the language ).
        /// </summary>
        /// <param name="word">The word to register</param>
        public void Register(string word)
        {
            this[word] = word;
        }
    }
}
