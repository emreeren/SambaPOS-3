using System;

namespace Fluentscript.Lib.AST.Core
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class ValueExpr : Expr
    {
        /// <summary>
        /// Name of the variable.
        /// </summary>
        public string Name;


        /// <summary>
        /// Datatype of the variable.
        /// </summary>
        public Type DataType;


        /// <summary>
        /// Value of the variable.
        /// </summary>
        public object Value;
    }
}
