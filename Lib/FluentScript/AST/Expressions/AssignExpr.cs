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
            // CASE 1: Assign variable.  a = 1
            if (this.VarExp.IsNodeType(NodeTypes.SysVariable))
            {
                AssignHelper.SetVariableValue(this.Ctx, this, _isDeclaration, this.VarExp, this.ValueExp);
            }
            // CASE 2: Assign member.    
            //      e.g. dictionary       :  user.name = 'kishore'
            //      e.g. property on class:  user.age  = 20
            else if (this.VarExp.IsNodeType(NodeTypes.SysMemberAccess))
            {
                AssignHelper.SetMemberValue(this.Ctx, this, this.VarExp, this.ValueExp);
            }
            // Case 3: Assign value to index: "users[0]" = <expression>;
            else if (this.VarExp.IsNodeType(NodeTypes.SysIndex))
            {
                AssignHelper.SetIndexValue(this.Ctx, this, this.VarExp, this.ValueExp);
            }
            return LObjects.Null;
        }
    }
}
