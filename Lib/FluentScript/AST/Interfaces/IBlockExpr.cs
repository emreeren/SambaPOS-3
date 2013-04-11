using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;

namespace Fluentscript.Lib.AST.Interfaces
{
    public interface IBlockExpr : IExpr
    {
        List<Expr> Statements { get; set; }
    }
}
