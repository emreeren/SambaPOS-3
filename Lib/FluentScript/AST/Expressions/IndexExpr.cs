using System;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Member access expressions for "." property or "." method.
    /// </summary>
    public class IndexExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="variableExp">The variable expression to use instead of passing in name of variable.</param>
        /// <param name="indexExp">The expression representing the index value to get</param>
        /// <param name="isAssignment">Whether or not this is part of an assigment</param>
        public IndexExpr(Expr variableExp, Expr indexExp, bool isAssignment)
        {
            this.Nodetype = NodeTypes.SysIndex;
            this.InitBoundary(true, "]");
            this.VariableExp = variableExp;
            this.IndexExp = indexExp;
            this.IsAssignment = isAssignment;
        }


        /// <summary>
        /// Expression representing the index
        /// </summary>
        public Expr IndexExp;


        /// <summary>
        /// The variable expression representing the list.
        /// </summary>
        public Expr VariableExp;


        /// <summary>
        /// The object to get the index value from. Used if ObjectName is null or empty.
        /// </summary>
        public object ListObject;


        /// <summary>
        /// Whether or not this member access is part of an assignment.
        /// </summary>
        public bool IsAssignment;


        /// <summary>
        /// Evaluate object[index]
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            var ndxVal = IndexExp.Evaluate();
            this.ListObject = VariableExp.Evaluate();

            // Check for empty objects.
            ExceptionHelper.NotNull(this, this.ListObject, "indexing");
            ExceptionHelper.NotNull(this, ndxVal, "indexing");

            var lobj = (LObject)this.ListObject;

            // CASE 1. Access 
            //      e.g. Array: users[0] 
            //      e.g. Map:   users['total']
            if(!this.IsAssignment)
            {
                var result = EvalHelper.AccessIndex(this.Ctx.Methods, this, lobj, (LObject)ndxVal);
                return result;
            }

            // CASE 2.  Assignment
            //      e.g. Array: users[0]        = 'john'
            //      e.g. Map:   users['total']  = 200
            // NOTE: In this case of assignment, return back a MemberAccess object descripting what is assign
            var indexAccess = new IndexAccess();
            indexAccess.Instance = lobj;
            indexAccess.MemberName = (LObject) ndxVal;
            return indexAccess;
        }
    }    
}
