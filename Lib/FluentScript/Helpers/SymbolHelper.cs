using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComLib.Lang.AST;
using ComLib.Lang.Core;
using ComLib.Lang.Types;

namespace ComLib.Lang.Helpers
{
    public class SymbolHelper
    {
        public static void ResetSymbolAsFunction(ISymbols symscope, string varname, LObject lobj)
        {
            // 1. Define the function in global symbol scope
            var lambda = lobj as LFunction;
            var funcExpr = lambda.Value as FunctionExpr;
            var symbol = new SymbolFunction(funcExpr.Meta);
            symbol.Name = varname;
            symbol.FuncExpr = funcExpr;
            symscope.Define(symbol);
        }
    }
}
