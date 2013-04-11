using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib._Core;
using Fluentscript.Lib._Core.Meta.Types;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class for "fluent" operations
    /// </summary>
    public class FluentHelper
    {
        /// <summary>
        /// Builds function names from multiple variables expressions.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<string> BuildMultiWordFunctionNames(List<string> ids)
        {
            var names = new List<string>();
            string name = ids[0];
            names.Add(name);
            for (int ndx = 1; ndx < ids.Count; ndx++)
            {
                var id = ids[ndx];
                name = name + " " + id;
                names.Add(name);
            }
            names.Reverse();
            return names;
        }        


        /// <summary>
        /// Finds a matching script function name from the list of strings representing identifiers.
        /// </summary>
        /// <param name="ctx">The context of the script</param>
        /// <param name="ids">List of strings representing identifier tokens</param>
        /// <returns></returns>
        public static FunctionLookupResult MatchFunctionName(Context ctx, List<Tuple<string,int>> ids)
        {
            var names = ids;
            var foundFuncName = string.Empty;
            var found = false;
            var tokenCount = 0;
            var memberMode = MemberMode.FunctionScript;
            for( int ndx = ids.Count - 1; ndx >=0; ndx-- )
            {
                // "refill inventory"
                var possible = ids[ndx];
                string funcName = possible.Item1;
                string funcNameWithUnderScores = funcName.Replace(' ', '_');

                // Case 1: "refill inventory" - exists with spaces
                if (ctx.Symbols.IsFunc(funcName))
                {
                    foundFuncName = funcName;
                }
                // Case 2: "refill_inventory" - replace space with underscore. 
                else if (ctx.Symbols.IsFunc(funcNameWithUnderScores))
                {
                    foundFuncName = funcNameWithUnderScores;
                }
                // Case 3: Check external functions
                else if (ctx.ExternalFunctions.Contains(funcName))
                {
                    memberMode = MemberMode.FunctionExternal;
                    foundFuncName = funcName;
                }

                if (!string.IsNullOrEmpty(foundFuncName))
                {
                    found = true;
                    tokenCount = possible.Item2;
                    break;
                }
            }
            // CASE 1: Not found
            if (!found ) return FunctionLookupResult.False;

            // CASE 2: Single word function
            if ((found && tokenCount == 1) && memberMode == MemberMode.FunctionScript)
            {
                var sym = ctx.Symbols.GetSymbol(foundFuncName) as SymbolFunction;
                var func = sym.FuncExpr as FunctionExpr;
                //var func = ctx.Functions.GetByName(foundFuncName);
                // If wildcard return true;
                if (func.Meta.HasWildCard)
                    return new FunctionLookupResult(true, foundFuncName, memberMode) { TokenCount = tokenCount };
                else
                    return FunctionLookupResult.False;
            }
                

            var result = new FunctionLookupResult()
            {
                Exists = found,
                Name = foundFuncName,
                FunctionMode = memberMode,
                TokenCount = tokenCount
            };
            return result;
        }


        /// <summary>
        /// Parses parameters.
        /// </summary>
        /// <param name="args">The list of arguments to store.</param>
        /// <param name="tokenIt">The token iterator</param>
        /// <param name="parser">The parser</param>
        /// <param name="meta">The function meta for checking parameters</param>
        /// <param name="expectParenthesis">Whether or not to expect parenthis to designate the start of the parameters.</param>
        /// <param name="enableNewLineAsEnd">Whether or not to treat a newline as end</param>
        public static void ParseFuncParameters(List<Expr> args, TokenIterator tokenIt, Parser.Parser parser, bool expectParenthesis, bool enableNewLineAsEnd, FunctionMetaData meta)
        {
            int totalParameters = 0;
            if (tokenIt.NextToken.Token == Tokens.LeftParenthesis)
                expectParenthesis = true;

            // START with check for "("
            if (expectParenthesis) tokenIt.Expect(Tokens.LeftParenthesis);

            bool passNewLine = !enableNewLineAsEnd;
            var endTokens = BuildEndTokens(enableNewLineAsEnd, meta);
            
            int totalNamedParams = 0;
            var hasMetaArguments = meta != null && meta.ArgumentNames != null && meta.ArgumentNames.Count > 0;
            while (true)
            {
                Expr exp = null;
            
                // Check for end of statment or invalid end of script.
                if (parser.IsEndOfParameterList(Tokens.RightParenthesis, enableNewLineAsEnd))
                    break;
                
                if (tokenIt.NextToken.Token == Tokens.Comma) 
                    tokenIt.Advance();
                
                var token = tokenIt.NextToken.Token;
                var peek = tokenIt.Peek().Token;

                var isVar = parser.Context.Symbols.Contains(token.Text);
                var isParamNameMatch = hasMetaArguments && meta.ArgumentsLookup.ContainsKey(token.Text);
                var isKeywordParamName = token.Kind == TokenKind.Keyword && isParamNameMatch;

                // CASE 1: Named params for external c# object method calls                
                // CASE 2: Named params for internal script functions ( where we have access to its param metadata )
                if (   (meta == null && token.Kind == TokenKind.Ident && peek == Tokens.Colon ) 
                    || (token.Kind == TokenKind.Ident && isParamNameMatch && !isVar) 
                    || (token.Kind == TokenKind.Ident && !isParamNameMatch && !isVar && peek == Tokens.Colon)
                    || (isKeywordParamName && !isVar ) )
                {         
                    var paramName = token.Text;
                    var namedParamToken = tokenIt.NextToken;
                    tokenIt.Advance();

                    // Advance and check if ":"
                    if (tokenIt.NextToken.Token == Tokens.Colon)
                        tokenIt.Advance();
                    
                    exp = parser.ParseExpression(endTokens, true, false, true, passNewLine, true);
                    exp = Exprs.NamedParam(paramName, exp, namedParamToken);

                    args.Add(exp);
                    totalNamedParams++;
                }
                // CASE 2: Name of variable being passed to function is same as one of the parameter names.
                else if (isVar && hasMetaArguments && meta.ArgumentsLookup.ContainsKey(token.Text))
                {
                    // Can not have normal parameters after named parameters.
                    if (totalNamedParams > 0)
                        throw tokenIt.BuildSyntaxException("Un-named parameters must come before named parameters");

                    var next = tokenIt.Peek();
                    if (next.Token.Kind == TokenKind.Symbol)
                        exp = parser.ParseExpression(endTokens, true, false, true, passNewLine, false);
                    else
                        exp = parser.ParseIdExpression(null, null, false);
                    args.Add(exp);
                }
                // CASE 3: Normal param
                else
                {
                    // Can not have normal parameters after named parameters.
                    if (totalNamedParams > 0)
                        throw tokenIt.BuildSyntaxException("Un-named parameters must come before named parameters");

                    exp = parser.ParseExpression(endTokens, true, false, true, passNewLine, true);
                    args.Add(exp);
                }                
                totalParameters++;
                parser.Context.Limits.CheckParserFunctionParams(exp, totalParameters);

                // Check for end of statment or invalid end of script.
                if (parser.IsEndOfParameterList(Tokens.RightParenthesis, enableNewLineAsEnd))
                    break;

                // Advance if not using fluent-parameters
                if(meta == null)
                    tokenIt.Expect(Tokens.Comma);
            }

            // END with check for ")"
            if (expectParenthesis) tokenIt.Expect(Tokens.RightParenthesis);
        }


        private static IDictionary<Token, bool> BuildEndTokens(bool enableNewLineAsEnd, FunctionMetaData meta)
        {
            var endTokens = new Dictionary<Token, bool>();
            var formalEndTokens = enableNewLineAsEnd ? Terminators.ExpFluentFuncExpParenEnd : Terminators.ExpFuncExpEnd;
            foreach (var pair in formalEndTokens)
                endTokens[pair.Key] = true;

            if (meta == null) return endTokens;

            // Go through all the arguments and use the 
            if (meta.ArgumentsLookup != null && meta.ArgumentsLookup.Count > 0)
            {
                // Add all the parameter names and aliases to the map.
                foreach (var pair in meta.ArgumentsLookup)
                {
                    var idToken = TokenBuilder.ToIdentifier(pair.Value.Name);
                    endTokens[idToken] = true;
                    if (!string.IsNullOrEmpty(pair.Value.Alias))
                    {
                        idToken = TokenBuilder.ToIdentifier(pair.Value.Alias);
                        endTokens[idToken] = true;
                    }
                }
            }
            return endTokens;
        }
    }
}
