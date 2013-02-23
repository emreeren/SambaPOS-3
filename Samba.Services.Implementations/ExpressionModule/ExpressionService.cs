using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Text.RegularExpressions;
using ComLib.Lang;
using ComLib.Lang.AST;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Persistance.DaoClasses;
using Samba.Services.Implementations.ExpressionModule.Accessors;

namespace Samba.Services.Implementations.ExpressionModule
{
    [Export(typeof(IExpressionService))]
    class ExpressionService : IExpressionService
    {
        private readonly IAutomationDao _automationDao;
        private readonly Interpreter _interpreter;
        private Dictionary<string, string> _scripts;
        private Dictionary<string, string> Scripts { get { return _scripts ?? (_scripts = _automationDao.GetScripts()); } }

        [ImportingConstructor]
        public ExpressionService(IAutomationDao automationDao)
        {
            _automationDao = automationDao;
            _interpreter = new Interpreter();
            _interpreter.SetFunctionCallback("Call", CallFunction);
            _interpreter.SetFunctionCallback("F", FormatFunction);
            _interpreter.SetFunctionCallback("TN", ToNumberFunction);

            _interpreter.LexReplace("Ticket", "TicketAccessor");
            _interpreter.LexReplace("Order", "OrderAccessor");
            _interpreter.LexReplace("Locator", "LocatorAccessor");
            _interpreter.Context.Plugins.RegisterAll();
            _interpreter.Context.Types.Register(typeof(LocatorAccessor), null);
            _interpreter.Context.Types.Register(typeof(TicketAccessor), null);
            _interpreter.Context.Types.Register(typeof(OrderAccessor), null);
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

        private object CallFunction(string s, string s1, FunctionCallExpr arg3)
        {
            return EvalCommand(s, null, null, default(object));
        }

        public string Eval(string expression)
        {
            try
            {
                _interpreter.Execute("result = " + expression);
                return _interpreter.Result.Success ? _interpreter.Memory.Get<string>("result") : "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public T Eval<T>(string expression, object dataObject, T defaultValue = default(T))
        {
            try
            {
                if (dataObject != null)
                {
                    TicketAccessor.Model = GetDataValue<Ticket>(dataObject);
                    OrderAccessor.Model = GetDataValue<Order>(dataObject);
                }
                _interpreter.Execute(expression);
                return _interpreter.Result.Success ? _interpreter.Memory.Get<T>("result") : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public T EvalCommand<T>(string functionName, IEntityClass entity, object dataObject, T defaultValue = default(T))
        {
            var entityName = entity != null ? "_" + entity.Name : "";
            var script = GetScript(functionName, entityName);
            if (string.IsNullOrEmpty(script)) return defaultValue;
            return Eval(script, dataObject, defaultValue);
        }

        public string ReplaceExpressionValues(string data, string template = "\\[=([^\\]]+)\\]")
        {
            var result = data;
            while (Regex.IsMatch(result, template, RegexOptions.Singleline))
            {
                var match = Regex.Match(result, template);
                var tag = match.Groups[0].Value;
                var expression = match.Groups[1].Value.Trim();
                if (expression.StartsWith("$") && !expression.Trim().Contains(" ") && _interpreter.Memory.Contains(expression.Trim('$')))
                {
                    result = result.Replace(tag, _interpreter.Memory.Get<string>(expression.Trim('$')));
                }
                else result = result.Replace(tag, Eval(expression));
            }
            return result;
        }

        private string GetScript(string functionName, string entityName)
        {
            if (Scripts.ContainsKey(functionName + entityName))
                return Scripts[functionName + entityName];
            if (Scripts.ContainsKey(functionName + "_*"))
                return Scripts[functionName + "_*"];
            return "";
        }

        private static T GetDataValue<T>(object dataObject) where T : class
        {
            if (dataObject == null) return null;
            var property = dataObject.GetType().GetProperty(typeof(T).Name);
            if (property != null)
                return property.GetValue(dataObject, null) as T;
            return null;
        }

        public void ResetCache()
        {
            _scripts = null;
        }
    }
}
