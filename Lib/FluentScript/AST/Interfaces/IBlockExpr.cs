using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLib.Lang.AST
{
    public interface IBlockExpr : IExpr
    {
        List<Expr> Statements { get; set; }
    }
}
