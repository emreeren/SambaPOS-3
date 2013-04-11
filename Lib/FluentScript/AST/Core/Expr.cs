using System;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.AST.Core
{
    /// <summary>
    /// Base class for Expressions
    /// </summary>
    public class Expr : AstNode, IExpr
    {
        /// <summary>
        /// Context information of the script.
        /// </summary>
        public Context Ctx { get; set; }


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
        /// Whether or not this expression has support for blocks, which hold multiple statements. like an if/while/for.
        /// </summary>
        public bool IsBlockSupported { get; set; }


        /// <summary>
        /// Whether or not a new scope is needed when entering the block.
        /// </summary>
        public bool IsNewScopeInBlock { get; set; }
        

        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public virtual object Evaluate(IAstVisitor visitor)
        {
            object result = null;
            if (this.Ctx != null && this.Ctx.Callbacks.HasAny)
            {
                Ctx.Callbacks.Notify("expression-on-before-execute", this, this);
                result = DoEvaluate(visitor);
                Ctx.Callbacks.Notify("expression-on-after-execute", this, this);
                return result;
            }
            result = DoEvaluate(visitor);
            return result;
        }


        /// <summary>
        /// Evaluate and return as datatype T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T EvaluateAs<T>(IAstVisitor visitor)
        {
            object result = Evaluate(visitor);

            // Evalulate<bool>() converting null to true.
            if (result == LObjects.Null && typeof(T) == typeof(bool))
                return default(T);
            if (result is LObject)
                return (T)Convert.ChangeType(((LObject) result).GetValue(), typeof(T), null);

            return (T)Convert.ChangeType(result, typeof(T), null);
        }


        public virtual object DoEvaluate(IAstVisitor visitor)
        {
            return null;
        }


        public virtual object Visit(IAstVisitor visitor)
        {
            return this.DoVisit(visitor);
        }

        
        public virtual object DoVisit(IAstVisitor visitor)
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
        public LangException BuildRunTimeException(string message)
        {
            return new LangException("Runtime Error", message, this.Ref.ScriptName, this.Ref.Line, this.Ref.CharPos);
        }
    }
}
