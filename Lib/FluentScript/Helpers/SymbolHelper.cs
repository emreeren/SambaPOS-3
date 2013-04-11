using Fluentscript.Lib.AST;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Helpers
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
