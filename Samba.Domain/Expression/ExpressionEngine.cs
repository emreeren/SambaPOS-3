﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ComLib.Lang;
using ComLib.Lang.AST;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Expression
{
    public static class ExpressionEngine
    {
        private static readonly Interpreter Interpreter;

        static ExpressionEngine()
        {
            Interpreter = new Interpreter();
            Interpreter.SetFunctionCallback("F", FormatFunction);
            Interpreter.SetFunctionCallback("TN", ToNumberFunction);

            Interpreter.LexReplace("Ticket", "TicketAccessor");
            Interpreter.LexReplace("Order", "OrderAccessor");
            Interpreter.Context.Plugins.RegisterAll();
            Interpreter.Context.Types.Register(typeof(TicketAccessor), null);
            Interpreter.Context.Types.Register(typeof(OrderAccessor), null);
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
            double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out d);
            return d;
        }

        private static object FormatFunction(string s, string s1, FunctionCallExpr arg3)
        {
            var fmt = !string.IsNullOrEmpty(s1) ? s1 : "#,#0.00";
            return (Convert.ToDouble(s)).ToString(fmt);
        }

        public static string Eval(string expression)
        {
            try
            {
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
            var property = dataObject.GetType().GetProperty(typeof(T).Name);
            if (property != null)
                return property.GetValue(dataObject, null) as T;
            return null;
        }
    }
}
