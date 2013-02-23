
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
    public class FunctionDeclarePlugin : ExprBlockPlugin, IParserCallbacks
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public FunctionDeclarePlugin()
        {
            this.Precedence = 2;
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
                    "function hour( num ) { ... }",
                    "function hours, hour, hrs, hr( num ) { ... }",
                    "function order_toBuy, 'order to buy'( num ) { ... }"
                };
            }
        }


        /// <summary>
        /// return value;
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            return Parse(_tokenIt.NextToken, true);
        }



        /// <summary>
        /// Parses a function declaration statement.
        /// This method is made public to allow other plugins to be used to allow different 
        /// words to represent "function" e.g. "def" instead of "function"
        /// </summary>
        /// <param name="token">The tokendata representing the starting token e.g. "function".</param>
        /// <param name="expectToken">Whether or not to expect the token in tokenData. 
        /// If false, advances the token iterator</param>
        /// <returns></returns>
        public Expr Parse(TokenData token, bool expectToken)
        {
            var stmt = new FunctionDeclareExpr();
            _parser.SetupContext(stmt.Function, token);

            if (expectToken) _tokenIt.Expect(token.Token);
            else _tokenIt.Advance();

            // Function name.
            var name = _tokenIt.ExpectId(true, true);
            var aliases = new List<string>();
            var nextToken = _tokenIt.NextToken;
            List<string> argNames = null;

            // Option 1: Wild card 
            if (nextToken.Token == Tokens.Multiply)
            {
                stmt.Function.Meta.HasWildCard = true;
                nextToken = _tokenIt.Advance();
            }
            // Option 2: Aliases
            else if (nextToken.Token == Tokens.Comma)
            {
                // Collect all function aliases
                while (nextToken.Token == Tokens.Comma)
                {
                    _tokenIt.Advance();
                    var alias = _tokenIt.ExpectId(true, true);
                    aliases.Add(alias);
                    nextToken = _tokenIt.NextToken;
                }
                if (aliases.Count > 0)
                    stmt.Function.Meta.Aliases = aliases;
            }

            // Get the parameters.
            if (nextToken.Token == Tokens.LeftParenthesis)
            {
                _tokenIt.Expect(Tokens.LeftParenthesis);
                argNames = _parser.ParseNames();
                _tokenIt.Expect(Tokens.RightParenthesis);
            }

            stmt.Function.Meta.Init(name, argNames);
            
            // Now parser the function block.
            ParseBlock(stmt.Function);
            
            return stmt;
        }


        /// <summary>
        /// Parses a block by first pushing symbol scope and then popping after completion.
        /// </summary>
        public override void ParseBlock(BlockExpr stmt)
        {
            var fs = stmt as FunctionExpr;
            var funcName = fs.Name;
            
            // 1. Define the function in global symbol scope
            var funcSymbol = new SymbolFunction(fs.Meta);
            funcSymbol.FuncExpr = stmt;

            this.Ctx.Symbols.Define(funcSymbol);

            // 2. Define the aliases.
            if (fs.Meta.Aliases != null && fs.Meta.Aliases.Count > 0)
                foreach (var alias in fs.Meta.Aliases)
                    this.Ctx.Symbols.DefineAlias(alias, fs.Meta.Name);
            
            // 3. Push the current scope.
            stmt.SymScope = this.Ctx.Symbols.Current;
            this.Ctx.Symbols.Push(new SymbolsFunction(fs.Name), true);

            // 4. Register the parameter names in the symbol scope.
            if( fs.Meta.Arguments != null && fs.Meta.Arguments.Count > 0)
                foreach(var arg in fs.Meta.Arguments)
                    this.Ctx.Symbols.DefineVariable(arg.Name, LTypes.Object);

            _parser.ParseBlock(stmt);
            this.Ctx.Symbols.Pop();
        }


        /// <summary>
        /// Called by the framework after the parse method is called
        /// </summary>
        /// <param name="node">The node returned by this implementations Parse method</param>
        public void OnParseComplete(AstNode node)        
        {
            var function = (node as FunctionDeclareExpr).Function;
            
            // 1. Register the function as a symbol
            this.Ctx.Symbols.DefineFunction(function.Meta, function);

            // 2. Now register the aliases
            if (function.Meta.Aliases != null && function.Meta.Aliases.Count > 0)
            {
                foreach (string alias in function.Meta.Aliases)
                {
                    this.Ctx.Symbols.DefineAlias(alias, function.Name);
                }
            }
        }
    }
}
