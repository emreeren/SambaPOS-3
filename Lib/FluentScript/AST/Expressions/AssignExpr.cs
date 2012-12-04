using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.Plugins
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class AssignExpr : Expr
    {
        private bool _isDeclaration;
        
        
        /// <summary>
        /// The variable expression
        /// </summary>
        public Expr VarExp;


        /// <summary>
        /// The value expression.
        /// </summary>
        public Expr ValueExp;


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="isDeclaration">Whether or not the variable is being declared in addition to assignment.</param>
        /// <param name="varExp">Expression representing the variable name to set</param>
        /// <param name="valueExp">Expression representing the value to set variable to.</param>
        public AssignExpr(bool isDeclaration, Expr varExp, Expr valueExp)
        {
            this.Nodetype = NodeTypes.SysAssign;            
            this._isDeclaration = isDeclaration;
            this.VarExp = varExp;
            this.ValueExp = valueExp;
        }
        

        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            return AssignHelper.AssignValue(this, this.VarExp, this.ValueExp, this._isDeclaration);
        }
    }
}
