using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class AssignMultiExpr : Expr
    {
        /// <summary>
        /// The declarations
        /// </summary>
        public List<AssignExpr> Assignments;


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="isDeclaration">Whether or not the variable is being declared in addition to assignment.</param>
        /// <param name="varExp">Expression representing the variable name to set</param>
        /// <param name="valueExp">Expression representing the value to set variable to.</param>
        public AssignMultiExpr(bool isDeclaration, Expr varExp, Expr valueExp)
        {
            this.Nodetype = NodeTypes.SysAssignMulti;
            this.Assignments = new List<AssignExpr>();
            this.Assignments.Add(new AssignExpr(true, varExp, valueExp));
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="isDeclaration">Whether or not the variable is being declared in addition to assignment.</param>
        /// <param name="declarations"></param>        
        public AssignMultiExpr(bool isDeclaration, List<AssignExpr> declarations)
        {
            this.Nodetype = NodeTypes.SysAssignMulti;
            this.Assignments = declarations;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitAssignMulti(this);
        }
    }
}
