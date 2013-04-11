using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // FuncWildCard plugin allows using functions with changing names allowing
    // a single function to handle function calls with different names.
     
    
    // @summary: A single function can be called using wildcards
    // @arg name: wildcard,  type: string, example: "by name password email role"
    // @arg name: wildcardParts, type: list, example: ['by', 'name' 'password', 'email', 'role' ]
    // @arg name: args, type: list, example: [ 'kreddy', 'admin' ]
    function "create user by" * ( wildcard, wildcardParts, args ) 
    {
	    person = new Person()
		
	    // Option 1: Use the full name to determine what to do
	    if wildcard== "name password email role" 
	    {
		    person.Name = args[0]
		    person.Password = args[1]
		    person.Email = args[2]
		    person.Role = args[3]
	    }
		
	    // Option 2: Use the individual name parts
	    for( var ndx = 0; ndx < wildcardParts.length; ndx++)
	    {
                    part = wildcardParts[ndx]

		    if part == "name" then person.Name = args[ndx]
		    else if part == "password" then person.Password = args[ndx]
		    else if part == "email"    then person.Email = args[ndx]
		    else if part == "role"     then person.Role = args[ndx]
	    }
	    person.Save()
    }


    create user by name email ( "user02", "user02@abc.com" )
    create user by name password email role ( "user01", "password", "user01@abc.com", "user" )
    
    </doc:example>
    ***************************************************************************/


    /// <summary>
    /// Combinator for handles method/function calls in a more fluent way.
    /// </summary>
    public class FuncWildCardPlugin : ExprPlugin
    {
        private FunctionLookupResult _result;

        /// <summary>
        /// Initialize.
        /// </summary>
        public FuncWildCardPlugin()
        {
            this.Precedence = 10;
            this.IsStatement = true;
            this.StartTokens = new string[] { "$IdToken" }; 
            this.IsContextFree = false;
        }


        /// <summary>
        /// This can not handle all idtoken based expressions.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {            
            if (!(current.Kind == TokenKind.Ident)) return false;

            var next = _tokenIt.Peek(1, false);
            if (!(next.Token.Kind == TokenKind.Ident)) return false;

            // Check if multi-word function name.
            // e.g. "refill inventory"
            // 1. Is it a function call?            
            var tokens = _tokenIt.PeekConsequetiveIdsAppendedWithTokenCounts(true, _tokenIt.LLK);
            _result = FluentHelper.MatchFunctionName(_parser.Context, tokens);
                        
            // Validate.
            // 1. The function must exist.
            if (!_result.Exists) return false;

            // 2. Only fluentscript functions support wildcard.
            if (_result.FunctionMode != MemberMode.FunctionScript) return false;
            
            // 3. Has wildcard flag must be turned on.
            var sym = _parser.Context.Symbols.GetSymbol(_result.Name) as SymbolFunction;
            var func = sym.FuncExpr as FunctionExpr;
            //var func = _parser.Context.Functions.GetByName(_result.Name);
            if (!func.Meta.HasWildCard) return false;

            return true;
        }


        private bool CheckIfSingleIdentWildCard(List<Tuple<string, int>> tokens)
        {
            var first = tokens[0].Item1;
            var isfunc = _parser.Context.Symbols.IsFunc(first);
            if (isfunc)
            {
                var sym = _parser.Context.Symbols.GetSymbol(_result.Name) as SymbolFunction;
                var func = sym.FuncExpr as FunctionExpr;
                //var func = _parser.Context.Functions.GetByName(first);
                if (func.Meta.HasWildCard)
                {
                    _result = new FunctionLookupResult(true, first, MemberMode.FunctionScript) { TokenCount = 1 };
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Parses the fluent function call.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // 1. Is it a function call?
            var fnameToken = _tokenIt.NextToken;

            _tokenIt.Advance(_result.TokenCount);

            string remainderOfFuncName = string.Empty;
            var parts = new List<Expr>();
            TokenData firstPart = null;

            // NOTES:
            // Given: function "Find user by" *
            // And: called via Find use by name role 
            // wildcard part 1: name
            // wildcard part 2: role
            // full wildcard: "name role"
            var partsToken = _tokenIt.NextToken;
            // 1. Capture all the remaining parts of the wild card.
            while (_tokenIt.NextToken.Token.Kind == TokenKind.Ident)
            {
                string part = _tokenIt.NextToken.Token.Text;
                
                // a. Store the token of the first wildcard part
                if(firstPart == null)
                    firstPart = _tokenIt.NextToken;

                // b. Build up the full name from all wildcards
                remainderOfFuncName += " " + part;

                // c. Create a constant expr from the wildcard
                // as it will be part of an array of strings passed to function
                var partExp = Exprs.Const(new LString(part), _tokenIt.NextToken);
                parts.Add(partExp);

                // d. Move to the next token for another possible wildcard.
                _tokenIt.Advance();

                // e. Check for end of statement.
                if (_tokenIt.IsEndOfStmtOrBlock())
                    break;
            }

            var exp = new FunctionCallExpr();
            exp.ParamListExpressions = new List<Expr>();
            exp.ParamList = new List<object>();
            remainderOfFuncName = remainderOfFuncName.Trim();
            var fullWildCard = Exprs.Const(new LString(string.Empty), fnameToken) as ConstantExpr;

            // 2. Create a constant expr representing the full wildcard              
            if(!string.IsNullOrEmpty(remainderOfFuncName))
            {
                fullWildCard.Value = remainderOfFuncName;
                _parser.SetupContext(fullWildCard, firstPart);
            }   

            var token = _tokenIt.NextToken.Token;

            // CASE 1: Parse parameters with parenthesis "("
            if (token == Tokens.LeftParenthesis)
            {
                _parser.ParseParameters(exp, true, false, false);
            }
            // CASE 2: Parse parameters with ":" until newline.
            else if (token == Tokens.Colon)
            {
                _tokenIt.Advance();
                _parser.ParseParameters(exp, false, false, true);
            }
            exp.NameExp = Exprs.Ident(_result.Name, fnameToken);
            // Have to restructure the arguments.
            // 1. const expr     , fullwildcard,   "name role"
            // 2. list<constexpr>, wildcard parts, ["name", "role"]
            // 3. list<expr>,      args,           "kishore", "admin"
            var args = new List<Expr>();
            args.Add(fullWildCard);
            args.Add(Exprs.Array(parts, partsToken));
            args.Add(Exprs.Array(exp.ParamListExpressions, fnameToken));

            // Finally reset the parameters expr on the function call.
            exp.ParamListExpressions = args;
            return exp;
        }
    }
}