using System;
using System.Collections.Generic;
using System.Reflection;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core.Meta.Types;

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class for function parameters.
    /// </summary>
    public class ParamHelper
    {
        /// <summary>
        /// Whether or not the parametlist of expressions contains a named parameter with the name supplied.
        /// </summary>
        /// <param name="paramListExpressions">List of parameter list expressions.</param>
        /// <param name="paramName">Name of the parameter to search for</param>
        /// <returns></returns>
        public static bool HasNamedParameter(List<Expr> paramListExpressions, string paramName)
        {
            if (paramListExpressions == null || paramListExpressions.Count == 0)
                return false;

            foreach (var paramExpr in paramListExpressions)
                if (paramExpr.IsNodeType(NodeTypes.SysNamedParameter))
                    if (((NamedParameterExpr)paramExpr).Name == paramName)
                        return true;
            return false;
        }


        /// <summary>
        /// Resolve all the non-named parameter expressions and puts the values into the param list supplied.
        /// </summary>
        public static void ResolveNonNamedParameters(List<Expr> paramListExpressions, List<object> paramList, IAstVisitor visitor)
        {
            if (paramListExpressions == null || paramListExpressions.Count == 0)
                return;

            paramList.Clear();
            foreach (var exp in paramListExpressions)
            {
                object val = exp.Evaluate(visitor);
                paramList.Add(val);
            }
        }


        /// <summary>
        /// Resolve the parameters in the function call.
        /// </summary>
        public static void ResolveParametersToHostLangValues(List<Expr> paramListExpressions, List<object> paramList, IAstVisitor visitor)
        {
            if (paramListExpressions == null || paramListExpressions.Count == 0)
                return;

            paramList.Clear();
            foreach (var exp in paramListExpressions)
            {
                var val = exp.Evaluate(visitor);
                if(val is LObject)
                {
                    var converted = ((LObject)val).GetValue();
                    paramList.Add(converted);
                }
                else
                    paramList.Add(val);
            }
        }


        /// <summary>
        /// Resolves the parameter expressions to actual values.
        /// </summary>
        /// <param name="totalParams">The total number of paramters possible</param>
        /// <param name="paramListExpressions">The list of expressions that should be evaulated first to pass as parameters</param>
        /// <param name="paramList">The list of parameters to a populate from paramListExpressions</param>
        /// <param name="indexLookup">The lookup callback to get the index position for named parameters</param>
        /// <param name="containsLookup">The lookup callback to determine if the namedparameter exists in the function. </param>
        /// <param name="visitor">The visitor that will evaulate the expressions.</param>
        public static void ResolveParameters(int totalParams, List<Expr> paramListExpressions, List<object> paramList, Func<NamedParameterExpr, int> indexLookup, 
            Func<NamedParameterExpr, bool> containsLookup, IAstVisitor visitor)
        {
            if (paramListExpressions == null || paramListExpressions.Count == 0)
                return;

            // 1. Determine if named params exist.
            bool hasNamedParams = ParamHelper.HasNamedParameters(paramListExpressions);

            // 2. If no named parameters, simply eval parameters and return.
            if (!hasNamedParams)
            {
                ResolveNonNamedParameters(paramListExpressions, paramList, visitor);
                return;
            }
            
            // Start of named parameter evaluation.
            // 1. Clear existing list of value.
            paramList.Clear();

            // 2. Set all args to null. [null, null, null, null, null]
            for (int ndx = 0; ndx < totalParams; ndx++)
                paramList.Add(LObjects.Null);

            // 3. Now go through each argument and replace the nulls with actual argument values.
            // Each null should be replaced at the correct index.
            // [true, 20.68, new Date(2012, 8, 10), null, 'fluentscript']
            for (int ndx = 0; ndx < paramListExpressions.Count; ndx++)
            {
                var exp = paramListExpressions[ndx];

                // 4. Named arg? Evaluate and put its value into the appropriate index of the args list.           
                if (exp.IsNodeType(NodeTypes.SysNamedParameter))
                {
                    var namedParam = exp as NamedParameterExpr;
                    object val = namedParam.Visit(visitor);
                    var contains = containsLookup(namedParam);
                    if (!contains)
                        throw ExceptionHelper.BuildRunTimeException(namedParam, "Named parameter : " + namedParam.Name + " does not exist");

                    int argIndex = indexLookup(namedParam);
                    paramList[argIndex] = val;
                }
                else
                {
                    // 5. Expect the position of non-named args should be valid.
                    // TODO: Semantic analysis is required here once Lint check feature is added.
                    object val = exp.Visit(visitor);
                    paramList[ndx] = val;
                }
            }
        }


        /// <summary>
        /// Resolve the parameters in the function call.
        /// </summary>
        /// <param name="meta">The function metadata</param>
        /// <param name="paramListExpressions">The list of expressions that should be evaulated first to pass as parameters</param>
        /// <param name="paramList">The list of parameters to a populate from paramListExpressions</param>
        /// <param name="visitor">The visitor that will evaulate the expressions.</param>
        public static void ResolveParametersForScriptFunction(FunctionMetaData meta, List<Expr> paramListExpressions, List<object> paramList, IAstVisitor visitor)
        {
            int totalParams = meta.Arguments == null ? 0 : meta.Arguments.Count;
            ResolveParameters(totalParams, paramListExpressions, paramList,
                namedParam => meta.ArgumentsLookup[namedParam.Name].Index,
                namedParam => meta.ArgumentsLookup.ContainsKey(namedParam.Name), visitor);
        }


        /// <summary>
        /// Resolve the parameters in the function call.
        /// </summary>
        /// <param name="method">The method that is being called. </param>
        /// <param name="paramListExpressions">The list of expressions that should be evaulated first to pass as parameters</param>
        /// <param name="paramList">The list of parameters to a populate from paramListExpressions</param>
        /// <param name="visitor">The visitor that will evaulate the expressions.</param>
        public static void ResolveParametersForMethodCall(MethodInfo method, List<Expr> paramListExpressions, List<object> paramList, IAstVisitor visitor)
        {
            var parameters = method.GetParameters();
            if (parameters == null || parameters.Length == 0) return;

            // 1. Convert parameters to map to know what index position in argument list a param is.
            var map = System.Linq.Enumerable.ToDictionary(parameters, p => p.Name);
            
            // 2. Resolve all the parameters to fluentscript values. LObject, LString etc.
            ResolveParameters(parameters.Length, paramListExpressions, paramList,
                namedParam => map[namedParam.Name].Position,
                namedParam => map.ContainsKey(namedParam.Name), visitor);
        }


        /// <summary>
        /// Whether or not there are named parameters here.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static bool HasNamedParameters(List<Expr> parameters)
        {
            if (parameters == null || parameters.Count == 0) return false;

            var hasNamedParams = false;
            foreach (var param in parameters)
            {
                if (param.IsNodeType(NodeTypes.SysNamedParameter))
                {
                    hasNamedParams = true;
                    break;
                }
            }
            return hasNamedParams;
        }
    }
}
