using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Types;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.AST.Core
{
    /// <summary>
    /// Variable expression data
    /// </summary>
    public class EmptyExpr : ValueExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public EmptyExpr()
        {
        }


        /// <summary>
        /// Evaluate value.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            return LObjects.Null;
        }
    }
}
