using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Plugins;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// Helper class for calling functions
    /// </summary>
    public class RegisteredFunctions: IFunctionLookup
    {
        private Dictionary<string, FunctionExpr> _functions;
        private Dictionary<string, string> _lcaseToFormaNameMap;


        /// <summary>
        /// Initialize
        /// </summary>
        public RegisteredFunctions()
        {
            _functions = new Dictionary<string, FunctionExpr>();
            _lcaseToFormaNameMap = new Dictionary<string, string>();
        }


        /// <summary>
        /// Registers a custom function callback.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="stmt">The function</param>
        public void Register(string pattern, FunctionExpr stmt)
        {
            _functions[pattern] = stmt;
            _lcaseToFormaNameMap[pattern.ToLower()] = pattern;
        }

        
        /// <summary>
        /// Whether or not the function name.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            return _functions.ContainsKey(name);
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
        public FunctionExpr GetByName(string name)
        {
            return _functions[name];
        }


        /// <summary>
        /// Calls the custom function.
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public object Call(FunctionCallExpr exp)
        {
            return CallByName(exp.Name, exp.ParamListExpressions, exp.ParamList, true);
        }



        /// <summary>
        /// Call a function by passing in all the values.
        /// </summary>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="paramListExpressions">List of parameters as expressions to evaluate first to actual values</param>
        /// <param name="paramVals">List to store the resolved paramter expressions. ( these will be resolved if paramListExpressions is supplied and resolveParams is true. If 
        /// resolveParams is false, the list is assumed to have the values for the paramters to the function.</param>
        /// <param name="resolveParams">Whether or not to resolve the list of parameter expression objects</param>
        /// <returns></returns>
        public object CallByName(string functionName, List<Expr> paramListExpressions, List<object> paramVals, bool resolveParams)
        {
            var function = GetByName(functionName);
            var hasParams = paramListExpressions != null && paramListExpressions.Count > 0;

            // 1. Resolve parameters if necessary
            if(resolveParams && function != null && ( function.HasArguments || hasParams ) )
                ParamHelper.ResolveParametersForScriptFunction(function.Meta, paramListExpressions, paramVals);
            function.ArgumentValues = paramVals;
            function.Evaluate();
            object result = null;
            if (function.HasReturnValue)
                result = function.ReturnValue;
            else
                result = LObjects.Null;
            return result;
        }
    }
}
