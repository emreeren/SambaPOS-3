using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.Integration;
using Fluentscript.Lib.Runtime;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class for calling functions in the script.
    /// </summary>
    public class FunctionHelper
    {
        /// <summary>
        /// Call a fluent script function from c#.
        /// </summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="expr">The lambda function</param>
        /// <param name="convertApplicableTypes">Whether or not to convert applicable c# types to fluentscript types, eg. ints and longs to double, List(object) to LArrayType and Dictionary(string, object) to LMapType</param>
        /// <param name="args"></param>
        public static object CallFunctionViaCSharpUsingLambda(Context context, FunctionExpr expr, bool convertApplicableTypes, params object[] args)
        {
            var argsList = args.ToList<object>();
            if (convertApplicableTypes)
                LangTypeHelper.ConvertToLangTypeValues(argsList);
            var execution = new Execution();
            execution.Ctx = context;
            if (EvalHelper.Ctx == null)
                EvalHelper.Ctx = context;

            var result = FunctionHelper.CallFunctionInScript(context, execution, expr.Meta.Name, expr, null, argsList, false);
            return result;
        }


        /// <summary>
        /// Call a fluent script function from c#.
        /// </summary>
        /// <param name="context">The context of the call.</param>
        /// <param name="functionName">The name of the function to call</param>
        /// <param name="convertApplicableTypes">Whether or not to convert applicable c# types to fluentscript types, eg. ints and longs to double, List(object) to LArrayType and Dictionary(string, object) to LMapType</param>
        /// <param name="args"></param>
        public static object CallFunctionViaCSharp(Context context, string functionName, bool convertApplicableTypes, params object[] args)
        {
            var exists = context.Symbols.IsFunc(functionName);
            if (!exists)
                return null;
            var sym = context.Symbols.GetSymbol(functionName) as SymbolFunction;
            var expr = sym.FuncExpr as FunctionExpr;
            return FunctionHelper.CallFunctionViaCSharpUsingLambda(context, expr, convertApplicableTypes, args);
        }


        /// <summary>
        /// Calls an internal function or external function.
        /// </summary>
        /// <param name="ctx">The context of the runtime.</param>
        /// <param name="fexpr">The function call expression</param>
        /// <param name="functionName">The name of the function. if not supplied, gets from the fexpr</param>
        /// <param name="pushCallStack"></param>
        /// <returns></returns>
        public static object CallFunction(Context ctx, FunctionCallExpr fexpr, string functionName, bool pushCallStack, IAstVisitor visitor)
        {
            if(string.IsNullOrEmpty(functionName))
                functionName = fexpr.NameExp.ToQualifiedName();

            // 1. Check if script func or extern func.
            var isScriptFunc = fexpr.SymScope.IsFunction(functionName);
            var isExternFunc = ctx.ExternalFunctions.Contains(functionName);

            // 2. If neither, this is an error scenario.
            if (!isScriptFunc && !isExternFunc)
                throw ExceptionHelper.BuildRunTimeException(fexpr, "Function does not exist : '" + functionName + "'");

            // 3. Push the name of the function on teh call stack
            if(pushCallStack)
                ctx.State.Stack.Push(functionName, fexpr);

            // 4. Call the function.
            object result = null;
            // Case 1: Custom C# function blog.create blog.*
            if (isExternFunc)
                result = FunctionHelper.CallFunctionExternal(ctx, visitor, functionName, fexpr);

            // Case 2: Script functions "createUser('john');" 
            else
            {
                var sym = fexpr.SymScope.GetSymbol(functionName) as SymbolFunction;
                var func = sym.FuncExpr as FunctionExpr;
                var resolveParams = !fexpr.RetainEvaluatedParams;
                result = FunctionHelper.CallFunctionInScript(ctx, visitor, functionName, func,  fexpr.ParamListExpressions,
                                                             fexpr.ParamList, resolveParams);
            }
            // 3. Finnaly pop the call stact.
            if(pushCallStack)
                ctx.State.Stack.Pop();

            result = CheckConvert(result);
            return result;
        }


        /// <summary>
        /// Calls a property get
        /// </summary>
        /// <param name="ctx">The context of the runtime</param>
        /// <param name="memberAccess">Object to hold all the relevant information required for the member call.</param>
        /// <param name="paramListExpressions">The collection of parameters as expressions</param>
        /// <param name="paramList">The collection of parameter values after they have been evaluated</param>
        /// <returns></returns>
        public static object CallMemberOnBasicType(Context ctx, AstNode node, MemberAccess memberAccess, List<Expr> paramListExpressions, List<object> paramList, IAstVisitor visitor)
        {
            object result = null;

            // 1. Get methods
            var methods = ctx.Methods.Get(memberAccess.Type);
            
            // 2. Get object on which method/property is being called on.
            var lobj = (LObject)memberAccess.Instance;

            // 3. Property ?
            if (memberAccess.Mode == MemberMode.PropertyMember)
            {
                result = methods.GetProperty(lobj, memberAccess.MemberName);
            }
            // 4. Method
            else if (memberAccess.Mode == MemberMode.MethodMember)
            {
                object[] args = null; 
                if(paramListExpressions != null && paramListExpressions.Count > 0)
                {
                    ParamHelper.ResolveNonNamedParameters(paramListExpressions, paramList, visitor);
                    args = paramList.ToArray();
                }
                result = methods.ExecuteMethod(lobj, memberAccess.MemberName, args);
            }
            result = CheckConvert(result);
            return result;
        }


        /// <summary>
        /// Execute a member call.
        /// </summary>
        /// <param name="ctx">The context of the script</param>
        /// <param name="memberAccess">Object to hold all the relevant information required for the member call.</param>
        /// <param name="paramListExpressions">The expressions to resolve as parameters</param>
        /// <param name="paramList">The list of parameters.</param>
        /// <returns></returns>
        public static object CallMemberOnClass(Context ctx, AstNode node, MemberAccess memberAccess, List<Expr> paramListExpressions, List<object> paramList, IAstVisitor visitor)
        {
            object result = LObjects.Null;
            var obj = memberAccess.Instance;
            var type = memberAccess.DataType;
            
            // Case 1: Property access
            if (memberAccess.Property != null)
            {
                var prop = type.GetProperty(memberAccess.MemberName);
                if (prop != null)
                    result = prop.GetValue(obj, null);
            }
            // Case 2: Method call.
            else if( memberAccess.Method != null)
            {
                result = FunctionHelper.MethodCall(ctx, obj, type, memberAccess.Method, paramListExpressions, paramList, true, visitor);
            }
            // Case 1: Property access
            if (memberAccess.Field != null)
            {
                result = memberAccess.Field.GetValue(obj);
            }
            result = CheckConvert(result);
            return result;
        }


        /// <summary>
        /// Call a function by passing in all the values.
        /// </summary>
        /// <param name="ctx">The context of the runtime</param>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="paramListExpressions">List of parameters as expressions to evaluate first to actual values</param>
        /// <param name="paramVals">List to store the resolved paramter expressions. ( these will be resolved if paramListExpressions is supplied and resolveParams is true. If 
        /// resolveParams is false, the list is assumed to have the values for the paramters to the function.</param>
        /// <param name="resolveParams">Whether or not to resolve the list of parameter expression objects</param>
        /// <returns></returns>
        public static object CallFunctionInScript(Context ctx, IAstVisitor visitor, 
            string functionName, FunctionExpr function, 
            List<Expr> paramListExpressions, List<object> paramVals, 
            bool resolveParams)
        {
            // 1. Determine if any parameters provided.
            var hasParams = paramListExpressions != null && paramListExpressions.Count > 0;

            // 2. Resolve parameters if necessary
            var hasArguments = function.Meta.HasArguments();
            if (resolveParams && function != null && (hasArguments || hasParams))
                ParamHelper.ResolveParametersForScriptFunction(function.Meta, paramListExpressions, paramVals, visitor);

            // 3. Assign the argument values to the function and evaluate.
            function.ArgumentValues = paramVals;
            visitor.VisitFunction(function);

            object result = null;
            if (function.HasReturnValue)
                result = function.ReturnValue;
            else
                result = LObjects.Null;
            return result;
        }


        /// <summary>
        /// Calls the custom function.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static object CallFunctionExternal(Context ctx, IAstVisitor visitor, string name, FunctionCallExpr exp )
        {
            var externalFuncs = ctx.ExternalFunctions;
            var objectName = name;
            var method = string.Empty;
            Func<string, string, FunctionCallExpr, object> callback = null;

            // Contains callback for full function name ? e.g. CreateUser
            if (externalFuncs.Contains(name))
                callback = externalFuncs.GetByName(name);

            // Contains callback that handles multiple methods on a "object".
            // e.g. Blog.Create, Blog.Delete etc.
            if (name.Contains("."))
            {
                var ndxDot = name.IndexOf(".");
                objectName = name.Substring(0, ndxDot);
                method = name.Substring(ndxDot + 1);
                if (externalFuncs.Contains(objectName + ".*"))
                    callback = externalFuncs.GetByName(objectName + ".*");
            }

            if (callback == null)
                return LObjects.Null;

            // 1. Resolve parameter froms expressions into Lang values.
            ParamHelper.ResolveParametersToHostLangValues(exp.ParamListExpressions, exp.ParamList, visitor);
            object result = callback(objectName, method, exp);
            return result;
        }


        /// <summary>
        /// Whether or not the name/member combination supplied is a script level function or an external C# function
        /// </summary>
        /// <param name="ctx">Context of script</param>
        /// <param name="name">Object name "Log"</param>
        /// <param name="member">Member name "Info" as in "Log.Info"</param>
        /// <returns></returns>
        //public static bool IsInternalOrExternalFunction2(Context ctx, string name, string member)
        //{
        //    string fullName = name;
        //    if (!string.IsNullOrEmpty(member))
        //        fullName += "." + member;
        //
        //    // Case 1: getuser() script function
        //    if (ctx.Functions.Contains(fullName) || ctx.ExternalFunctions.Contains(fullName))
        //        return true;
        //
        //    return false;
        //}


        /// <summary>
        /// Whether or not this variable + member name maps to an external function call.
        /// Note: In fluentscript you can setup "Log.*" and allow all method calls to "Log" to map to that external call.
        /// </summary>
        /// <param name="funcs">The collection of external functions.</param>
        /// <param name="varName">The name of the external object e.g. "Log" as in "Log.Error"</param>
        /// <param name="memberName">The name of the method e.g. "Error" as in "Log.Error"</param>
        /// <returns></returns>
        public static bool IsExternalFunction(ExternalFunctions funcs, string varName, string memberName)
        {
            string funcName = varName + "." + memberName;
            if (funcs.Contains(funcName))
                return true;
            return false;
        }


        /// <summary>
        /// Dynamically invokes a method call.
        /// </summary>
        /// <param name="ctx">Context of the script</param>
        /// <param name="obj">Instance of the object for which the method call is being applied.</param>
        /// <param name="datatype">The datatype of the object.</param>
        /// <param name="methodInfo">The method to call.</param>
        /// <param name="paramListExpressions">List of expressions representing parameters for the method call</param>
        /// <param name="paramList">The list of values(evaluated from expressions) to call.</param>
        /// <param name="resolveParams">Whether or not to resolve the parameters from expressions to values.</param>
        /// <returns></returns>
        private static object MethodCall(Context ctx, object obj, Type datatype, MethodInfo methodInfo, List<Expr> paramListExpressions, List<object> paramList, bool resolveParams, IAstVisitor visitor)
        {
            // 1. Convert language expressions to values.
            if (resolveParams) 
                ParamHelper.ResolveParametersForMethodCall(methodInfo, paramListExpressions, paramList, visitor);

            // 2. Convert internal language types to c# code method types.
            object[] args = LangTypeHelper.ConvertArgs(paramList, methodInfo);

            // 3. Handle  params object[];
            if (methodInfo.GetParameters().Length == 1)
            {
                if (methodInfo.GetParameters()[0].ParameterType == typeof(object[]))
                    args = new object[] { args };
            }
            object result = methodInfo.Invoke(obj, args);
            return result;
        }


        public static object CheckConvert(object result)
        {
            // Finally, convert to fluentscript types.
            // Case 1: Aleady an LObject
            if (result is LObject)
                return result;

            // Case 2: C# type so wrap inside of fluentscript type.
            return LangTypeHelper.ConvertToLangValue(result);
        }
    }
}
