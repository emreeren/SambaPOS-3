using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
// </lang:using>

namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// A combinator to extend the parser
    /// </summary>
    public class ExprPlugin : ExprPluginBase, IExprPlugin
    {
        protected ExprParser _exprParser;

        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <returns></returns>
        public virtual Expr Parse()
        {
            return Parse(null);
        }


        /// <summary>
        /// Parses using the contextual object supplied.
        /// </summary>
        /// <param name="context">Contextual object for passing information into the parse method.</param>
        /// <returns></returns>
        public virtual Expr Parse(object context)
        {
            return null;
        }


        public virtual void OnParseComplete(AstNode node)
        {
            
        }
    }
}
