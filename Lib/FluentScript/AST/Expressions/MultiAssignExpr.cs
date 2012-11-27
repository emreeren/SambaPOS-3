using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.AST;
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.Plugins
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class MultiAssignExpr : Expr
    {
        /// <summary>
        /// The declarations
        /// </summary>
        internal List<AssignExpr> _assignments;


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="isDeclaration">Whether or not the variable is being declared in addition to assignment.</param>
        /// <param name="varExp">Expression representing the variable name to set</param>
        /// <param name="valueExp">Expression representing the value to set variable to.</param>
        public MultiAssignExpr(bool isDeclaration, Expr varExp, Expr valueExp)
        {
            this.Nodetype = NodeTypes.SysAssignMulti;
            this._assignments = new List<AssignExpr>();
            this._assignments.Add(new AssignExpr(true, varExp, valueExp));
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="isDeclaration">Whether or not the variable is being declared in addition to assignment.</param>
        /// <param name="declarations"></param>        
        public MultiAssignExpr(bool isDeclaration, List<AssignExpr> declarations)
        {
            this.Nodetype = NodeTypes.SysAssignMulti;
            this._assignments = declarations;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            foreach (var assigment in _assignments)
            {
                assigment.Evaluate();
            }
            return LObjects.Null;
        }
    }
}
