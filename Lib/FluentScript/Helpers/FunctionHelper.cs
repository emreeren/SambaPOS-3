using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace ComLib.Lang.Helpers
{
    /// <summary>
    /// Helper class for calling functions in the script.
    /// </summary>
    public class FunctionHelper
    {
        /// <summary>
        /// Whether or not the name/member combination supplied is a script level function or an external C# function
        /// </summary>
        /// <param name="ctx">Context of script</param>
        /// <param name="name">Object name "Log"</param>
        /// <param name="member">Member name "Info" as in "Log.Info"</param>
        /// <returns></returns>
        public static bool IsInternalOrExternalFunction(Context ctx, string name, string member)
        {
            string fullName = name;
            if (!string.IsNullOrEmpty(member))
                fullName += "." + member;

            // Case 1: getuser() script function
            if (ctx.Functions.Contains(fullName) || ctx.ExternalFunctions.Contains(fullName))
                return true;

            return false;
        }        


        /// <summary>
        /// Call internal/external script.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static object FunctionCall(Context ctx, string name, FunctionCallExpr exp)
        {
            // Case 1: Custom C# function blog.create blog.*
            if (ctx.ExternalFunctions.Contains(name))
                return ctx.ExternalFunctions.Call(name, exp);

            // Case 2: Script functions "createUser('john');" 
            return ctx.Functions.Call(exp);
        }


        /// <summary>
        /// Execute a member call.
        /// </summary>
        /// <param name="ctx">The context of the script</param>
        /// <param name="type">The type of the object</param>
        /// <param name="obj">The object to call the method on</param>
        /// <param name="varname">The name of the variable</param>
        /// <param name="memberName">The name of the member/method to call</param>
        /// <param name="methodInfo">The methodinfo(not needed for built in types )</param>
        /// <param name="paramListExpressions">The expressions to resolve as parameters</param>
        /// <param name="paramList">The list of parameters.</param>
        /// <returns></returns>
        public static object MemberCall(Context ctx, Type type, object obj, string varname, string memberName, MethodInfo methodInfo, List<Expr> paramListExpressions, List<object> paramList)
        {
            // 1. Resolve the parameters.
            if(methodInfo == null)
                FunctionHelper.ResolveParameters(paramListExpressions, paramList);

            object result = null;
            if (type == null && obj != null)
                type = obj.GetType();

            // 1. DateTime
            if (type == typeof(DateTime))
            {
                result = new LDate(ctx, varname, (DateTime)obj).ExecuteMethod(memberName, paramList.ToArray());
            }
            // 2. String
            else if (type == typeof(string))
            {
                result = new LString(ctx, varname, (string)obj).ExecuteMethod(memberName, paramList.ToArray());
            }
            // 3. Method info supplied
            else if (methodInfo != null)
            {
                result = MethodCall(ctx, obj, type, methodInfo, paramListExpressions, paramList, true);
            }
            else
            {
                methodInfo = type.GetMethod(memberName);
                if (methodInfo != null)
                    result = methodInfo.Invoke(obj, paramList.ToArray());
                else
                {
                    var prop = type.GetProperty(memberName);
                    if(prop != null)
                        result = prop.GetValue(obj, null);
                }
            }
            return result;
        }


        /// <summary>
        /// Resolve the parameters in the function call.
        /// </summary>
        public static void ResolveParameters(List<Expr> paramListExpressions, List<object> paramList)
        {
            if (paramListExpressions == null || paramListExpressions.Count == 0)
                return;

            paramList.Clear();
            foreach (var exp in paramListExpressions)
            {
                object val = exp.Evaluate();
                paramList.Add(val);
            }
        }


        /// <summary>
        /// Resolve the parameters in the function call.
        /// </summary>
        public static void ResolveParametersForScriptFunction(FunctionMetaData meta, List<Expr> paramListExpressions, List<object> paramList)
        {
            int totalParams = meta.Arguments == null ? 0 : meta.Arguments.Count;
            ResolveParameters(totalParams, paramListExpressions, paramList, 
                namedParam => meta.ArgumentsLookup[namedParam.Name].Index);
        }


        /// <summary>
        /// Resolve the parameters in the function call.
        /// </summary>
        public static void ResolveParametersForMethodCall(MethodInfo method, List<Expr> paramListExpressions, List<object> paramList)
        {
            var parameters = method.GetParameters();
            if (parameters == null || parameters.Length == 0) return;

            // Convert parameters to map.
            var map = parameters.ToDictionary(p => p.Name);

            ResolveParameters(parameters.Length, paramListExpressions, paramList,
                namedParam => map[namedParam.Name].Position);
        }  


        /// <summary>
        /// Prints to the console.
        /// </summary>
        /// /// <param name="settings">Settings for interpreter</param>
        /// <param name="exp">The functiona call expression</param>
        /// <param name="printline">Whether to print with line or no line</param>
        public static string Print(LangSettings settings, FunctionCallExpr exp, bool printline)
        {
            if (!settings.EnablePrinting) return string.Empty;

            string message = BuildMessage(exp.ParamList);
            if (printline) Console.WriteLine(message);
            else Console.Write(message);
            return message;
        }


        /// <summary>
        /// Logs severity to console.
        /// </summary>
        /// <param name="settings">Settings for interpreter</param>
        /// <param name="exp">The functiona call expression</param>
        public static string Log(LangSettings settings, FunctionCallExpr exp)
        {
            if (!settings.EnableLogging) return string.Empty;

            string severity = exp.Name.Substring(exp.Name.IndexOf(".") + 1);
            string message = BuildMessage(exp.ParamList);
            Console.WriteLine(severity.ToUpper() + " : " + message);
            return message;
        }


        /// <summary>
        /// Builds a single message from multiple arguments
        /// If there are 2 or more arguments, the 1st is a format, then rest are the args to the format.
        /// </summary>
        /// <param name="paramList">The list of parameters</param>
        /// <returns></returns>
        public static string BuildMessage(List<object> paramList)
        {
            string val = string.Empty;
            bool hasFormat = false;
            string format = string.Empty;
            if (paramList != null && paramList.Count > 0)
            {
                // Check for 2 arguments which reflects formatting the printing.
                hasFormat = paramList.Count > 1;
                if (hasFormat)
                {
                    format = paramList[0].ToString();
                    var args = paramList.GetRange(1,paramList.Count - 1);
                    val = string.Format(format, args.ToArray());
                }
                else
                    val = paramList[0].ToString();
            }
            return val;
        }


        /// <summary>
        /// Converts arguments from one type to another type that is required by the method call.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="method">The method for which the parameters need to be converted</param>
        public static void ConvertArgs(List<object> args, MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters == null || parameters.Length == 0) return;

            // For each param
            for (int ndx = 0; ndx < parameters.Length; ndx++)
            {
                var param = parameters[ndx];
                object sourceArg = args[ndx];

                // types match ? continue to next one.
                if (sourceArg.GetType() == param.ParameterType)
                    continue;

                // 1. Double to Int32
                if (sourceArg.GetType() == typeof(double) && param.ParameterType == typeof(int))
                    args[ndx] = Convert.ToInt32(sourceArg);

                // 2. Double to Int32
                if (sourceArg.GetType() == typeof(double) && param.ParameterType == typeof(long))
                    args[ndx] = Convert.ToInt64(sourceArg);

                // 3. LDate to datetime
                else if (sourceArg is LDate)
                    args[ndx] = ((LDate)sourceArg).Raw;

                // 4. Null
                else if (sourceArg == LNull.Instance)                
                    args[ndx] = GetDefaultValue(param.ParameterType);

                // 5. LArray
                else if ((sourceArg is LArray || sourceArg is List<object>) && param.ParameterType.IsGenericType)
                {
                    if (sourceArg is LArray) sourceArg = ((LArray)sourceArg).Raw;
                    var gentype = param.ParameterType.GetGenericTypeDefinition();
                    if (gentype == typeof(List<>) || gentype == typeof(IList<>))
                    {
                        args[ndx] = ConvertToTypedList((List<object>)sourceArg, param.ParameterType);
                    }
                }
            }
        }


        /// <summary>
        /// Converts from c# datatypes to fluentscript datatypes.
        /// </summary>
        /// <param name="args"></param>
        public static void ConvertToFluentScriptTypes(List<object> args)
        {
            if (args == null || args.Count == 0)
                return;

            // Convert types from c# types fluentscript compatible types.
            for (int ndx = 0; ndx < args.Count; ndx++)
            {
                var val = args[ndx];
                if (val == null)
                    args[ndx] = LNull.Instance;
                else if (val.GetType() == typeof(int))
                    args[ndx] = Convert.ToDouble(val);
                else if (val.GetType() == typeof(List<object>))
                    args[ndx] = new LArray((List<object>)val);
                else if (val.GetType() == typeof(Dictionary<string, object>))
                    args[ndx] = new LMap((Dictionary<string, object>)val);

                // TODO: Need to handle other types such as List<T>, Dictionary<string, T> etc.
            }
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
        private static object MethodCall(Context ctx, object obj, Type datatype, MethodInfo methodInfo, List<Expr> paramListExpressions, List<object> paramList, bool resolveParams = true)
        {
            // 1. Convert language expressions to values.
            if (resolveParams) ResolveParametersForMethodCall(methodInfo, paramListExpressions, paramList);

            // 2. Convert internal language types to c# code method types.
            ConvertArgs(paramList, methodInfo);

            // 3. Now get args as an array for method calling.
            object[] args = paramList.ToArray();

            // 4. Handle  params object[];
            if (methodInfo.GetParameters().Length == 1)
            {
                if (methodInfo.GetParameters()[0].ParameterType == typeof(object[]))
                    args = new object[] { args };
            }
            object result = methodInfo.Invoke(obj, args);
            return result;
        }


        /// <summary>
        /// Converts the source to the target list type by creating a new instance of the list and populating it.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetListType"></param>
        /// <returns></returns>
        static object ConvertToTypedList(IList<object> source, Type targetListType)
        {
            var t = targetListType; // targetListType.GetType();
            var dt = targetListType.GetGenericTypeDefinition();
            var targetType = dt.MakeGenericType(t.GetGenericArguments()[0]);
            var targetList = Activator.CreateInstance(targetType);
            System.Collections.IList l = targetList as System.Collections.IList;
            foreach (var item in source) l.Add(item);
            return targetList;
        }


        /// <summary>
        /// Converts the source to the target list type by creating a new instance of the list and populating it.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetListType"></param>
        /// <returns></returns>
        static object ConvertToTypedDictionary(IDictionary<string, object> source, Type targetListType)
        {
            var t = targetListType; // targetListType.GetType();
            var dt = targetListType.GetGenericTypeDefinition();
            var targetType = dt.MakeGenericType(t.GetGenericArguments()[0], t.GetGenericArguments()[1]);
            var targetDict = Activator.CreateInstance(targetType);
            System.Collections.IDictionary l = targetDict as System.Collections.IDictionary;
            foreach (var item in source) l.Add(item.Key, item.Value);
            return targetDict;
        }


        private static void ResolveParameters(int totalParams, List<Expr> paramListExpressions, List<object> paramList, Func<NamedParamExpr, int> indexLookup)
        {
            if (paramListExpressions == null || paramListExpressions.Count == 0)
                return;

            paramList.Clear();
            bool hasNamedParams = false;
            foreach (var param in paramListExpressions)
                if (param is NamedParamExpr)
                    hasNamedParams = true;

            // If there are no named params. Simply evaluate and return.
            if (!hasNamedParams)
            {
                foreach (var exp in paramListExpressions)
                {
                    object val = exp.Evaluate();
                    paramList.Add(val);
                }
                
                return;
            }

            // 1. Set all args to null. [null, null, null, null, null]
            for (int ndx = 0; ndx < totalParams; ndx++)
                paramList.Add(null);

            // 2. Now go through each argument and replace the nulls with actual argument values.
            // Each null should be replaced at the correct index.
            // [true, 20.68, new Date(2012, 8, 10), null, 'fluentscript']
            for (int ndx = 0; ndx < paramListExpressions.Count; ndx++)
            {
                var exp = paramListExpressions[ndx];

                // 3. Named arg? Evaluate and put its value into the appropriate index of the args list.           
                if (exp is NamedParamExpr)
                {
                    var namedParam = exp as NamedParamExpr;
                    object val = namedParam.Evaluate();
                    int argIndex = indexLookup(namedParam);
                    paramList[argIndex] = val;
                }
                else
                {
                    // 4. Expect the position of non-named args should be valid.
                    // TODO: Semantic analysis is required here once Lint check feature is added.
                    object val = exp.Evaluate();
                    paramList[ndx] = val;
                }
            }
        }


        private static object GetDefaultValue(Type type)
        {
            if (type == typeof(int)) return 0;
            if (type == typeof(bool)) return false;
            if (type == typeof(double)) return 0.0;
            if (type == typeof(DateTime)) return DateTime.MinValue;
            if (type == typeof(TimeSpan)) return TimeSpan.MinValue;
            return null;
        }
    }
}
