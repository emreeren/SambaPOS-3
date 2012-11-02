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
    public class ConstantExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public ConstantExpr(object val)
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
            return this.Value;
        }
    }
}
