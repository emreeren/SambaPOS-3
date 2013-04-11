using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;

namespace Fluentscript.Lib.AST.Interfaces
{
    /// <summary>
    /// Interface for expression that uses parameters. right now "new" and "function".
    /// </summary>
    public interface IParameterExpression
    {
        /// <summary>
        /// List of evaluated parameters
        /// </summary>
        List<object> ParamList { get; set; }


        /// <summary>
        /// List of expressions representing the parameters.
        /// </summary>
        List<Expr> ParamListExpressions { get; set; }


        string ToQualifiedName();
    }
}
