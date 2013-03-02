using System;

// <lang:using>
using ComLib.Lang.Types;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Represents a literal( true, false, null, 'abc', 10 ) expression in code.
    /// Note: This will eventually replace the ConstantExpr.
    /// </summary>
    public class LiteralExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LiteralExpr(object val)
        {
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

            // 2. Actual value.
            var ltype = LangTypeHelper.ConvertToLangValue(this.Value);
            return ltype;
        }
    }
}
