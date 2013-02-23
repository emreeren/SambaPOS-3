using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComLib.Lang.Core;
using ComLib.Lang.Parsing;

namespace ComLib.Lang.AST
{
    public interface IExpr
    {
        ISymbols SymScope { get; set; }
        Context Ctx { get; set; }
        ScriptRef Ref { get; set; }
        TokenData Token { get; set; }
    }
}
