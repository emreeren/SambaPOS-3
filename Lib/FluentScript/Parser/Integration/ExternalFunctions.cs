using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
// <lang:using>

// </lang:using>


namespace Fluentscript.Lib.Parser.Integration
{
    /// <summary>
    /// Helper class for calling functions
    /// </summary>
    public class ExternalFunctions
    {
        private Dictionary<string, Func<string, string, FunctionCallExpr, object>> _customCallbacks;
        private Dictionary<string, string> _lcaseToFormaNameMap;


        /// <summary>
        /// Initialize
        /// </summary>
        public ExternalFunctions()
        {
            _customCallbacks = new Dictionary<string, Func<string, string, FunctionCallExpr, object>>();
            _lcaseToFormaNameMap = new Dictionary<string, string>();
        }


        /// <summary>
        /// Registers a custom function callback.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="callback">The custom callback</param>
        public void Register(string pattern, Func<string, string, FunctionCallExpr, object> callback)
        {
            _customCallbacks[pattern] = callback;
            _lcaseToFormaNameMap[pattern.ToLower()] = pattern;
        }

        
        /// <summary>
        /// Whether or not the function call supplied is a custom function callback that is 
        /// outside of the script.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            var callback = GetByName(name);
            return callback != null;
        }


        /// <summary>
        /// Get the formal case sensitive function name that matches the case insensitive function name supplied.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <returns></returns>
        public string GetMatch(string name)
        {
            if (!_lcaseToFormaNameMap.ContainsKey(name))
                return null;
            return _lcaseToFormaNameMap[name];
        }


        /// <summary>
        /// Get the custom function callback
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <returns></returns>
        public Func<string, string, FunctionCallExpr, object> GetByName(string name)
        {
            // Contains callback for full function name ? e.g. CreateUser
            if (_customCallbacks.ContainsKey(name))
                return _customCallbacks[name];
                    

            // Contains callback that handles multiple methods on a "object".
            // e.g. Blog.Create, Blog.Delete etc.
            if (name.Contains("."))
            {
                var prefix = name.Substring(0, name.IndexOf("."));
                if (_customCallbacks.ContainsKey(prefix + ".*"))
                    return _customCallbacks[prefix + ".*"];
            }
            return null;
        }   
    }
}
