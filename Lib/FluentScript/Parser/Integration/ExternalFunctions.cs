﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>


namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// Helper class for calling functions
    /// </summary>
    public class ExternalFunctions : IFunctionLookup
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


        /// <summary>
        /// Calls the custom function.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public object Call(string name, FunctionCallExpr exp, IAstVisitor visitor)
        {
            var objectName = name;
            var method = string.Empty;
            Func<string, string, FunctionCallExpr, object> callback = null;

            // Contains callback for full function name ? e.g. CreateUser
            if (_customCallbacks.ContainsKey(name))
                callback = _customCallbacks[name];

            // Contains callback that handles multiple methods on a "object".
            // e.g. Blog.Create, Blog.Delete etc.
            if (name.Contains("."))
            {
                var ndxDot = name.IndexOf(".");
                objectName = name.Substring(0, ndxDot);
                method = name.Substring(ndxDot + 1);
                if (_customCallbacks.ContainsKey(objectName + ".*"))
                    callback = _customCallbacks[objectName + ".*"];
            }

            if (callback == null)
                return LObjects.Null;
            
            // 1. Resolve parameter froms expressions into Lang values.
            ParamHelper.ResolveParametersToHostLangValues(exp.ParamListExpressions, exp.ParamList, visitor);
            object result = callback(objectName, method, exp);
            return result;
        }
    }
}
