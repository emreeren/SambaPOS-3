
// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
// </lang:using>

namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// A combinator to extend the parser
    /// </summary>
    public class ExprBlockPlugin : ExprPlugin
    {
        /// <summary>
        /// Parses a block by first pushing symbol scope and then popping after completion.
        /// </summary>
        public virtual void ParseBlock(BlockExpr stmt)
        {
            this.Ctx.Symbols.Push(new SymbolsNested(string.Empty), true);
            stmt.SymScope = this.Ctx.Symbols.Current;
            _parser.ParseBlock(stmt);
            this.Ctx.Symbols.Pop();
        }


        /// <summary>
        /// Parses a conditional block by first pushing symbol scope and then popping after completion.
        /// </summary>
        /// <param name="stmt"></param>
        public virtual void ParseConditionalBlock(ConditionalBlockExpr stmt)
        {
            this.Ctx.Symbols.Push(new SymbolsNested(string.Empty), true);
            stmt.SymScope = this.Ctx.Symbols.Current;
            _parser.ParseConditionalStatement(stmt);
            this.Ctx.Symbols.Pop();
        }
    }
}
