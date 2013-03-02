using System;

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class AssignExpr : Expr
    {
        public bool IsDeclaration;
        
        
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
            this.IsDeclaration = isDeclaration;
            this.VarExp = varExp;
            this.ValueExp = valueExp;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitAssign(this);
        }
    }
}
