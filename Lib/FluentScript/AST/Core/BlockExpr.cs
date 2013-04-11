using System;
using System.Collections.Generic;
using System.Text;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Types;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.AST.Core
{
    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class BlockExpr : Expr, IBlockExpr
    {
        /// <summary>
        /// Whether or not to enable auto variables ( used in for each loops )
        /// </summary>
        public bool EnableAutoVariable { get; set; }


        /// <summary>
        /// List of statements
        /// </summary>
        protected List<Expr> _statements = new List<Expr>();


        /// <summary>
        /// Public access to statments.
        /// </summary>
        public List<Expr> Statements
        {
            get { return _statements; }
            set { _statements = value; }
        }



        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object Evaluate(IAstVisitor visitor)
        {
            object result = null;
            if (this.Ctx != null && this.Ctx.Callbacks.HasAny)
            {
                Ctx.Callbacks.Notify("expression-on-before-execute", this, this);
                result = ExecuteBlock(visitor);
                Ctx.Callbacks.Notify("expression-on-after-execute", this, this);
                return result;
            }
            result = ExecuteBlock(visitor);
            return result;
        }


        /// <summary>
        /// Execute the statements.
        /// </summary>
        public override object  DoEvaluate(IAstVisitor visitor)
        {
            LangHelper.Evaluate(this._statements, this.Parent, visitor);
            return LObjects.Null;
        }


        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitBlock(this);
        }


        /// <summary>
        /// Executes the block with callback/template methods.
        /// </summary>
        protected virtual object ExecuteBlock(IAstVisitor visitor)
        {
            object result = LObjects.Null;
            try
            {
                OnBlockEnter(visitor);
                result = DoEvaluate(visitor);
            }
            finally
            {
                OnBlockExit(visitor);
            }
            return result;
        }


        /// <summary>
        /// On enter of the block.
        /// </summary>
        public virtual void OnBlockEnter(IAstVisitor visitor)
        {
            visitor.VisitBlockEnter(this); 
        }


        /// <summary>
        /// On exit of the block.
        /// </summary>
        public virtual void OnBlockExit(IAstVisitor visitor)
        {
            visitor.VisitBlockExit(this); 
        }


        /// <summary>
        /// String representation
        /// </summary>
        /// <param name="tab">Tab to use for nested statements in blocks</param>
        /// <param name="incrementTab">Whether or not to add another tab</param>
        /// <param name="includeNewLine">Whether or not to include a new line.</param>
        /// <returns></returns>
        public override string AsString(string tab = "", bool incrementTab = false,  bool includeNewLine = true)
        {
            string info = base.AsString(tab, incrementTab);

            // Empty statements?
            if (_statements == null || _statements.Count == 0) return info;

            var buffer = new StringBuilder();

            // Now iterate over all the statements in the block
            foreach (var stmt in _statements)
            {
                buffer.Append(stmt.AsString(tab, true));
            }

            var result = info + buffer.ToString();
            if (includeNewLine) result += Environment.NewLine;

            return result;
        }
    }
}
