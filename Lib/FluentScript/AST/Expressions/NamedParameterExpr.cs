using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class NamedParamExpr : Expr
    {
        /// <summary>
        /// The name of the expression.
        /// </summary>
        public string Name;


        /// <summary>
        /// The expression representing the value of the parameter..
        /// </summary>
        public Expr Value;


        /// <summary>
        /// Position of the named arg.
        /// </summary>
        public int Pos;


        /// <summary>
        /// Initialize
        /// </summary>
        public NamedParamExpr() : this(null, null)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">The expression representing the value of the parameter.</param>
        public NamedParamExpr(string name, Expr value)
        {
            this.Name = name;
            this.Value = value;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            var result = this.Value == null ? null : this.Value.Evaluate();
            return result;
        }


        /// <summary>
        /// Returns the fully qualified name of this node.
        /// </summary>
        /// <returns></returns>
        public override string ToQualifiedName()
        {
            return this.Name;
        }
    }
}
