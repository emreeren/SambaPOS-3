using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// Base class for Expressions
    /// </summary>
    public class Expr : AstNode
    {
        /// <summary>
        /// Empty expr.
        /// </summary>
        public static readonly Expr Empty = new Expr();


        /// <summary>
        /// The symbol scope associated w/ this instance.
        /// </summary>
        public ISymbols SymScope { get; set; }


        /// <summary>
        /// Whether or not this statement can be executed immediately at parsing time
        /// e.g. useful for function declaration statements among other future planned features.
        /// </summary>
        public bool IsImmediatelyExecutable { get; set; }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public virtual object Evaluate()
        {
            object result = null;
            if (this.Ctx != null && this.Ctx.Callbacks.HasAny)
            {
                Ctx.Callbacks.Notify("expression-on-before-execute", this, this);
                result = DoEvaluate();
                Ctx.Callbacks.Notify("expression-on-after-execute", this, this);
                return result;
            }
            result = DoEvaluate();
            return result;
        }


        /// <summary>
        /// Evaluate and return as datatype T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T EvaluateAs<T>()
        {
            object result = Evaluate();

            // Evalulate<bool>() converting null to true.
            if (result == LNull.Instance && typeof(T) == typeof(bool))
                return default(T);

            return (T)Convert.ChangeType(result, typeof(T), null);
        }


        /// <summary>
        /// Internal method to wrap statement executions around the callbacks.
        /// </summary>
        public virtual object DoEvaluate()
        {
            return null;
        }


        /// <summary>
        /// String representation of statement.
        /// </summary>
        /// <param name="tab">Tab to use</param>
        /// <param name="incrementTab">Whether or not to add another tab</param>
        /// <param name="includeNewLine">Whether or not to include a new line.</param>
        /// <returns></returns>
        public virtual string AsString(string tab = "", bool incrementTab = false, bool includeNewLine = true)
        {
            var stmtType = this.GetType().Name.Replace("Expr", "");
            string info = string.Format("{0}, {1}, {2} ", stmtType, Ref.Line, Ref.CharPos);

            if (incrementTab)
                tab = tab + "\t";

            var result = tab + info;
            if (includeNewLine) result += Environment.NewLine;

            return result;
        }


        /// <summary>
        /// Build a language exception due to the current token being invalid.
        /// </summary>
        /// <returns></returns>
        protected LangException BuildRunTimeException(string message)
        {
            return new LangException("Runtime Error", message, this.Ref.ScriptName, this.Ref.Line, this.Ref.CharPos);
        }
    }
}
