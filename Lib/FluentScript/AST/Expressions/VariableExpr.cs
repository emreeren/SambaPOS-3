using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class VariableExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public VariableExpr()
        {
            this.Nodetype = NodeTypes.SysVariable;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">Variable name</param>
        public VariableExpr(string name)
        {
            this.Nodetype = NodeTypes.SysVariable;
            this.Name = name;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            // Case 1: memory variable has highest precendence
            if (this.Ctx.Memory.Contains(this.Name))
            {
                this.Value = this.Ctx.Memory.Get<object>(this.Name);
                return this.Value;
            }
            // Case 2: check function now.
            if (this.SymScope.IsFunction(this.Name))
            {
                
            }
            throw this.BuildRunTimeException("variable : " + this.Name + " does not exist");
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
