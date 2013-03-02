using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ComLib.Lang.Types;
using ComLib.Lang.Helpers;


namespace ComLib.Lang.AST
{

    /// <summary>
    /// Expression to handle string interpolations such as "${username}'s email is ${email}".
    /// </summary>
    public class InterpolatedExpr : Expr
    {
        private List<Expr> _expressions;


        /// <summary>
        /// Initialize
        /// </summary>
        public InterpolatedExpr()
        {
            this.Nodetype = NodeTypes.SysInterpolated;
        }


        /// <summary>
        /// Adds an expression to be interpolated.
        /// </summary>
        /// <param name="exp"></param>
        public void Add(Expr exp)
        {
            if (_expressions == null)
                _expressions = new List<Expr>();

            _expressions.Add(exp);
        }


        /// <summary>
        /// Clears the expression.
        /// </summary>
        public void Clear()
        {
            if (_expressions != null)
                _expressions.Clear();
        }


        public List<Expr> Expressions { get { return _expressions; } }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitInterpolated(this);
        }
    }
}
