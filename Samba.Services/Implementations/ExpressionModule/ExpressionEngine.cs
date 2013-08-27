using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.Parser;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Services.Implementations.ExpressionModule.Accessors;

namespace Samba.Services.Implementations.ExpressionModule
{
    public static class ExpressionEngine
    {
        private static readonly Interpreter Interpreter;

        static ExpressionEngine()
        {
            Interpreter = new Interpreter();
            Interpreter.SetFunctionCallback("F", FormatFunction);
            Interpreter.SetFunctionCallback("TN", ToNumberFunction);
            Interpreter.SetFunctionCallback("FF", FixFormatFunction);
            Interpreter.SetFunctionCallback("Between", BetweenFunction);
            Interpreter.LexReplace("Ticket", "TicketAccessor");
            Interpreter.LexReplace("Order", "OrderAccessor");
            Interpreter.LexReplace("Entity", "EntityAccessor");
            Interpreter.LexReplace("Data", "DataAccessor");
            Interpreter.LexReplace("Helper", "HelperAccessor");
            Interpreter.LexReplace("is", "==");
            Interpreter.LexReplace("True", "true");
            Interpreter.LexReplace("False", "false");
            Interpreter.Context.Plugins.RegisterAll();
            Interpreter.Context.Types.Register(typeof(TicketAccessor), null);
            Interpreter.Context.Types.Register(typeof(OrderAccessor), null);
            Interpreter.Context.Types.Register(typeof(EntityAccessor), null);
            Interpreter.Context.Types.Register(typeof(DataAccessor), null);
            Interpreter.Context.Types.Register(typeof(HelperAccessor), null);
        }

        public static void RegisterType(Type type, string name)
        {
            Interpreter.LexReplace(type.Name, name);
            Interpreter.Context.Types.Register(type, null);
        }

        public static void RegisterFunction(string name, Func<string, string, FunctionCallExpr, object> function)
        {
            Interpreter.SetFunctionCallback(name, function);
        }

        private static object ToNumberFunction(string s, string s1, FunctionCallExpr arg3)
        {
            double d;
            double.TryParse(arg3.ParamList[0].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out d);
            return d;
        }

        private static object BetweenFunction(string arg1, string arg2, FunctionCallExpr arg3)
        {
            if (arg3.ParamList.Count == 0) return int.MaxValue;
            return int.MaxValue;
        }

        private static object FormatFunction(string s, string s1, FunctionCallExpr arg3)
        {
            var fmt = arg3.ParamList.Count > 1 ? arg3.ParamList[1].ToString() : "#,#0.00";
            return (Convert.ToDouble(arg3.ParamList[0])).ToString(fmt);
        }

        private static object FixFormatFunction(string s, string s1, FunctionCallExpr args)
        {
            double d;
            double.TryParse(args.ParamList[0].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out d);
            var fmt = args.ParamList.Count > 1 ? args.ParamList[1].ToString() : "#,#0.00";
            return d.ToString(fmt);
        }

        public static string Eval(string expression, object dataObject = null)
        {
            try
            {
                if (dataObject != null)
                {
                    TicketAccessor.Model = GetDataValue<Ticket>(dataObject);
                    OrderAccessor.Model = GetDataValue<Order>(dataObject);
                    EntityAccessor.Model = GetDataValue<Entity>(dataObject);
                    DataAccessor.Model = dataObject;
                }
                Interpreter.Execute("result = " + expression);
                return Interpreter.Result.Success ? Interpreter.Memory.Get<string>("result") : "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static T Eval<T>(string expression, object dataObject, T defaultValue = default(T))
        {
            try
            {
                if (dataObject != null)
                {
                    TicketAccessor.Model = GetDataValue<Ticket>(dataObject);
                    OrderAccessor.Model = GetDataValue<Order>(dataObject);
                    EntityAccessor.Model = GetDataValue<Entity>(dataObject);
                    DataAccessor.Model = dataObject;
                }
                Interpreter.Execute(expression);
                return Interpreter.Result.Success ? Interpreter.Memory.Get<T>("result") : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string ReplaceExpressionValues(string data, string template = "\\[=([^\\]]+)\\]")
        {
            var result = data;
            while (Regex.IsMatch(result, template, RegexOptions.Singleline))
            {
                var match = Regex.Match(result, template);
                var tag = match.Groups[0].Value;
                var expression = match.Groups[1].Value.Trim();
                expression = Regex.Unescape(expression);
                if (expression.StartsWith("$") && !expression.Trim().Contains(" ") && Interpreter.Memory.Contains(expression.Trim('$')))
                {
                    result = result.Replace(tag, Interpreter.Memory.Get<string>(expression.Trim('$')));
                }
                else result = result.Replace(tag, Eval(expression));
            }
            return result;
        }

        private static T GetDataValue<T>(object dataObject) where T : class
        {
            if (dataObject == null) return null;
            if (!((IDictionary<string, object>)dataObject).ContainsKey(typeof(T).Name)) return null;
            return ((IDictionary<string, object>)dataObject)[typeof(T).Name] as T;
        }

        public static string ReplaceExpressionValues(string data, object dataObject, string template = "\\[=([^\\]]+)\\]")
        {
            var result = data;
            while (Regex.IsMatch(result, template, RegexOptions.Singleline))
            {
                var match = Regex.Match(result, template);
                var tag = match.Groups[0].Value;
                var expression = match.Groups[1].Value.Trim();
                expression = Regex.Unescape(expression);
                if (expression.StartsWith("$") && !expression.Trim().Contains(" ") && Interpreter.Memory.Contains(expression.Trim('$')))
                {
                    result = result.Replace(tag, Interpreter.Memory.Get<string>(expression.Trim('$')));
                }
                else result = result.Replace(tag, Eval(expression, dataObject));
            }
            return result;
        }
    }
}
