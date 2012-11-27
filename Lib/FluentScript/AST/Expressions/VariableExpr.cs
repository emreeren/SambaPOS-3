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
            if (!this.Ctx.Memory.Contains(this.Name))
                throw this.BuildRunTimeException("variable : " + this.Name + " does not exist");

            this.Value = this.Ctx.Memory.Get<object>(this.Name);
            //this.DataType = this.Value.GetType();
            return this.Value;
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
