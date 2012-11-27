using System;

// <lang:using>
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class ConstantExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public ConstantExpr(object val)
        {
            this.Nodetype = NodeTypes.SysConstant;
            this.Value = val;
            this.DataType = val.GetType();
        }


        /// <summary>
        /// Evaluate value.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            // 1. Null
            if (this.Value == LObjects.Null)
                return this.Value;

            if (this.Value is LObject)
                return this.Value;

            // 2. Actual value.
            var ltype = LangTypeHelper.ConvertToLangValue(this.Value);
            return ltype;
        }
    }
}
