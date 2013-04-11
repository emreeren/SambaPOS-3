using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.AST.Interfaces
{
    public interface IExpr
    {
        ISymbols SymScope { get; set; }
        Context Ctx { get; set; }
        ScriptRef Ref { get; set; }
        TokenData Token { get; set; }
    }
}
