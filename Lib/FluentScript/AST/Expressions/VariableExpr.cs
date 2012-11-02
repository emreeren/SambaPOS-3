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
    public class VariableExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public VariableExpr()
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">Variable name</param>
        public VariableExpr(string name)
        {
            this.Name = name;
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            this.Value = this.Ctx.Memory.Get<object>(this.Name);
            this.DataType = this.Value.GetType();
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
