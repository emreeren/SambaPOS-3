
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
{
    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class LambdaPlugin : ExprBlockPlugin, IParserCallbacks
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public LambdaPlugin()
        {
            this.Precedence = 1;
            this.ConfigureAsSystemStatement(true, false, "function");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "function ( <id> | <stringliteral> ) ( ',' ( <id> | <stringliteral> ) )* <statementblock>"; }
        }


        /// <summary>
        /// Examples
        /// </summary>
        public override string[] Examples
        {
            get
            {
                return new string[]
                {
                    "function ( num ) { return num + 1; }"
                };
            }
        }


        public override bool CanHandle(Token current)
        {
            if (current != Tokens.Function) return false;
            var next = _tokenIt.Peek();
            if (next.Token == Tokens.LeftParenthesis)
                return true;
            return false;
        }


        /// <summary>
        /// return value;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var token = _tokenIt.NextToken;
            var exp = new FunctionExpr();
            _parser.SetupContext(exp, token);
            var name = "anon_" + token.Line + "_" + token.LineCharPos;
            _tokenIt.Advance();
            _tokenIt.Expect(Tokens.LeftParenthesis);
            var argnames = _parser.ParseNames();
            exp.Meta.Init(name, argnames);
            _tokenIt.Expect(Tokens.RightParenthesis);
            ParseBlock(exp);
            var lambdaExp = new LambdaExpr();
            lambdaExp.Expr = exp;
            return lambdaExp;
        }


        /// <summary>
        /// Parses a block by first pushing symbol scope and then popping after completion.
        /// </summary>
        public override void ParseBlock(BlockExpr stmt)
        {
            var fs = stmt as FunctionExpr;

            // 1. Define the function in global symbol scope
            var funcSymbol = new SymbolFunction(fs.Meta);
            funcSymbol.FuncExpr = stmt;

            // 2. Push the current scope.
            stmt.SymScope = this.Ctx.Symbols.Current;
            this.Ctx.Symbols.Push(new SymbolsFunction(string.Empty), true);

            // 3. Parse the function block
            _parser.ParseBlock(stmt);

            // 4. Pop the symbols scope.
            this.Ctx.Symbols.Pop();
        }


        /// <summary>
        /// Called by the framework after the parse method is called
        /// </summary>
        /// <param name="node">The node returned by this implementations Parse method</param>
        public void OnParseComplete(AstNode node)        
        {
            var function = (node as FunctionExpr);
            
            // 1. Register the function as a symbol
            //this.Ctx.Symbols.DefineFunction(function.Meta, function);
        }
    }
}
