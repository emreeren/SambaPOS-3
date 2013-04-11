using System;
using System.Collections.Generic;
using System.Globalization;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Plugins.Core;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Parser.MetaPlugins
{
    /*
     *  1. check for empty grammar
        2. check for invalid grammar
        3. throw on invalid compiler_plugin
        4. check how visit.evaluate works for lambda
        5. check for token advance
        6. type conversions
        7. simplify method calls / functions ( performance improvement )
        8. multi plugin support: IsMatch() to handle duplicate start tokens
        9. chaing plugins e.g. #ref:time  
       10. set up metacompiler tokens/context earlier on
       11. make sure plugins also support statements ?
       12. support for "this" in plugin to eventually have plugins in javascript as classes
       13. support for new function()   to eventually have plugins in javascript as classes       
    */
    public class MetaPluginContainer
    {
        private IDictionary<string, List<CompilerPlugin>> _pluginExprs;
        private IDictionary<string, List<CompilerPlugin>> _pluginTokens;
        private IDictionary<string, List<CompilerPlugin>> _pluginLexTokens;
        private List<CompilerPlugin> _allPlugins;
        private const string _typeDateToken = "$DateToken";
        private const string _typeNumberToken = "$NumberToken";
        private const string _typeIdentSymbolToken = "$IdentSymbolToken";

 
        private MatchResult _matchResult;
        private MatchResult _matchTokenResult;
        public MatchResult EmtpyMatch = new MatchResult(false, null, null);


        public MetaPluginContainer()
        {
            _pluginExprs = new Dictionary<string, List<CompilerPlugin>>();
            _pluginTokens = new Dictionary<string, List<CompilerPlugin>>();
            _pluginLexTokens = new Dictionary<string, List<CompilerPlugin>>();
            _allPlugins = new List<CompilerPlugin>();
        }

        public Lexer Lex;
        
        public ExprParser Parser;        

        public TokenIterator TokenIt;

        public TokenReplacePlugin LastMatchedTokenPlugin;

        public Symbols Symbols;


        /// <summary>
        /// Register the compiler plugin.
        /// </summary>
        /// <param name="plugin"></param>
        public void Register(CompilerPlugin plugin)
        {
            _allPlugins.Add(plugin);
            if (!plugin.IsEnabled)
                return;

            if (plugin.PluginType == "expr" && plugin.StartTokens.Length > 0)
            {
                foreach (var startToken in plugin.StartTokens)
                {
                    var tokenPlugins = _pluginExprs.ContainsKey(startToken)
                                           ? _pluginExprs[startToken]
                                           : new List<CompilerPlugin>();
                    tokenPlugins.Add(plugin);
                    _pluginExprs[startToken] = tokenPlugins;
                }
            }
            else if (plugin.PluginType == "lexer" && plugin.StartTokens.Length > 0)
            {
                var list = new List<CompilerPlugin>();
                list.Add(plugin);
                foreach (var startToken in plugin.StartTokens)
                {
                    _pluginLexTokens[startToken] = list;
                }
            }
            else if(plugin.PluginType == "token" )
            {
                var tplugin = new TokenReplacePlugin();

                var hasStartTokens = plugin.StartTokens != null && plugin.StartTokens.Length > 0;
                var list = new List<CompilerPlugin>();
                plugin.Handler = tplugin;
                list.Add(plugin);

                if( hasStartTokens )
                {
                    foreach(var startToken in plugin.StartTokens)
                    {
                        _pluginTokens[startToken] = list;
                    }
                }
                if (plugin.TokenReplacements != null && plugin.TokenReplacements.Count > 0)
                {
                    foreach (var replacements in plugin.TokenReplacements)
                    {
                        var alias = replacements[0];
                        var replaceWith = replacements[1];
                        tplugin.SetupReplacement(alias, replaceWith);
                        if(!hasStartTokens)
                        {
                            _pluginTokens[alias] = list;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Iterates through each plugin.
        /// </summary>
        /// <param name="callback"></param>
        public void EachPlugin(Action<CompilerPlugin> callback)
        {
            foreach (var plugin in _allPlugins)
            {
                callback(plugin);
            }
        }


        /// <summary>
        /// Total plugins.
        /// </summary>
        /// <returns></returns>
        public int TotalExprs()
        {
            return _pluginExprs == null ? 0 : _pluginExprs.Count;
        }


        /// <summary>
        /// The total number of tokens.
        /// </summary>
        /// <returns></returns>
        public int TotalTokens()
        {
            return _pluginTokens == null ? 0 : _pluginTokens.Count;
        }


        /// <summary>
        /// The total number of tokens.
        /// </summary>
        /// <returns></returns>
        public int TotalLex()
        {
            return _pluginLexTokens == null ? 0 : _pluginLexTokens.Count;
        }


        public bool ContainsTok(Token token, int tokenPos)
        {
            return this.CanHandleTok(token, tokenPos == 0);
        }


        /// <summary>
        /// Validates the compiler plugin.
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        public BoolMsgObj Validate(CompilerPlugin plugin)
        {
            var errors = new List<string>();
            if(string.IsNullOrEmpty(plugin.Grammar))
                errors.Add("Grammar not supplied");

            if(string.IsNullOrEmpty(plugin.Name))
                errors.Add("Compiler plugin name not supplied");

            if(string.IsNullOrEmpty(plugin.FullName))
                errors.Add("Compiler plugin full name not supplied");

            if(plugin.StartTokens.Length == 0)
                errors.Add("Start tokens not supplied");

            if(string.IsNullOrEmpty(plugin.PluginType))
                errors.Add("Plugin type not supplied");

            if(plugin.BuildExpr == null)
                errors.Add("Plugin parse function not supplied");

            var success = errors.Count == 0;
            var message = "";
            if (!success)
            {
                foreach (var msg in errors)
                    message += msg + "\r\n";
            }
            var result = new BoolMsgObj(errors, success, message);
            return result;
        }

        
        /// <summary>
        /// Whether or not there is a compiler plugin that can handle the token provided.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CanHandleExp(Token token)
        {
            // Avoid literal strings 
            if (token.Kind == TokenKind.LiteralString)
                return false;

            List<CompilerPlugin> plugins = null;

            // Start token exists to trigger plugin check?
            if (_pluginExprs.ContainsKey(token.Text))
            {
                plugins = _pluginExprs[token.Text];
            }
            else if (token.Kind == TokenKind.LiteralNumber)
            {
                if (_pluginExprs.ContainsKey(_typeNumberToken))
                    plugins = _pluginExprs[_typeNumberToken];
            }
            else if (token.Kind == TokenKind.Ident)
            {
                if (_pluginExprs.ContainsKey(_typeIdentSymbolToken))
                {
                    if (this.Symbols.Contains(token.Text))
                    {
                        plugins = _pluginExprs[_typeIdentSymbolToken];
                    }
                }
            }
            if (plugins == null)
                return false;
            
            _matchResult = EmtpyMatch;
            _matchResult = IsMatch(plugins);
            return _matchResult.Success;
        }


        public bool CanHandleLex(Token token)
        {
            if (!_pluginLexTokens.ContainsKey(token.Text))
                return false;
            var plugins = _pluginLexTokens[token.Text];
            foreach(var plugin in plugins)
            {
                
            }
            return true;
        }


        public bool CanHandleTok(Token token, bool isCurrent)
        {
            List<CompilerPlugin> plugins = null;

            // Case 1: Can not overtake identifiers that are actually variables(symbols) in current scope.
            if (token.Kind == TokenKind.Ident && this.Symbols != null && this.Symbols.Contains(token.Text))
                return false;

            // Case 2: Ident - specific word
            if (token.Kind == TokenKind.Ident && _pluginTokens.ContainsKey(token.Text))
            {
                plugins = _pluginTokens[token.Text];
            }
            // Case 3: General symbol e.g. $Ident $DateToken $Time
            else if(token.Kind == TokenKind.LiteralDate)
            {
                if (_pluginTokens.ContainsKey(_typeDateToken))
                    plugins = _pluginTokens[_typeDateToken];
            }
            if (plugins == null)
                return false;

            _matchTokenResult = new MatchResult(false, null, null);
            foreach (var plugin in plugins)
            {
                if (string.IsNullOrEmpty(plugin.GrammarMatch))
                {
                    var tokenPlugin = plugin.Handler as TokenReplacePlugin;
                    tokenPlugin.TokenIt = this.TokenIt;
                    if (tokenPlugin.CanHandle(token, isCurrent))
                    {
                        _matchTokenResult = new MatchResult(true, plugin, null);
                        this.LastMatchedTokenPlugin = tokenPlugin;
                        break;
                    }
                }
                else
                {
                    var result = IsGrammarMatchOnExpression(plugin);
                    if(result.Success)
                    {
                        _matchTokenResult = result;
                        break;
                    }
                }
            }
            return _matchTokenResult.Success;
        }


        /// <summary>
        /// Visit expression.
        /// </summary>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public Expr ParseExp(IAstVisitor visitor)
        {
            var expr = _matchResult.Plugin.BuildExpr as FunctionExpr;
            var ctx = expr.Ctx;

            // 1. Apply defaults
            var defaults = _matchResult.Plugin.ParseDefaults;
            if (defaults != null && defaults.Count > 0)
            {
                // 2. Go throug all defaults specified
                foreach (var pair in defaults)
                {
                    // 3. Not there ? Apply default.
                    if (!_matchResult.Args.ContainsKey(pair.Key))
                    {
                        _matchResult.Args[pair.Key] = pair.Value;
                    }
                }
            }

            // 2. Call the parse function on them metaplugin.
            var result = FunctionHelper.CallFunctionViaCSharpUsingLambda(ctx, expr, true, _matchResult.Args);
            var rexpr = ((LClass) result).Value as Expr;

            // Advance the tokens.
            this.TokenIt.Advance(_matchResult.TokenCount);

            // 3. Was the last just for grammar matching ? now continue with expression parsing?
            if (_matchResult.Plugin.MatchesForParse != null)
            {
                var plugin = _matchResult.Plugin;
                rexpr = this.ParseExpressionGrammar(rexpr, plugin, plugin.MatchesForParse, 0);
            }
            return rexpr;
        }


        public Token ParseLex(IAstVisitor visitor)
        {
            return null;
        }


        /// <summary>
        /// Visit expression.
        /// </summary>
        /// <returns></returns>
        public Token ParseTokens()
        {
            var plugin = _matchTokenResult.Plugin;
            if(!string.IsNullOrEmpty(plugin.GrammarMatch))
            {
                var ctx = plugin.BuildExpr.Ctx;
                var expr = plugin.BuildExpr as FunctionExpr;
                var result = FunctionHelper.CallFunctionViaCSharpUsingLambda(ctx, expr, true, _matchTokenResult.Args);
                var token = ((LClass) result).Value as Token;
                this.TokenIt.Advance(_matchTokenResult.TokenCount -1);
                return token;
            }
            var tokenReplacer = plugin.Handler as TokenReplacePlugin;
            return tokenReplacer.Parse();
        }


        /// <summary>
        /// Whether or not there is a plugin that matches the current tokens.
        /// </summary>
        /// <returns></returns>
        public MatchResult IsMatch()
        {
            var token = this.TokenIt.NextToken;

            // 1. no start token?
            if (!this._pluginExprs.ContainsKey(token.Token.Text))
                return this.EmtpyMatch;

            // 2. Get matching plugin.
            var plugins = this._pluginExprs[token.Token.Text];
            return IsMatch(plugins);
        }


        public MatchResult IsMatch(List<CompilerPlugin> plugins)
        {
            // 3. Check for matching plugin.
            var result = this.EmtpyMatch;
            foreach (var plugin in plugins)
            {
                // 4. auto match plugin?
                if (plugin.IsAutoMatched)
                {
                    result = new MatchResult(true, plugin, null);
                    return result;
                }

                // 5. Check Grammer.
                result = IsGrammarMatchOnExpression(plugin);
                if (result.Success)
                {
                    break;
                }
            }
            return result;
        }


        private MatchResult IsGrammarMatchOnExpression(CompilerPlugin plugin)
        {
            // 5. Check Grammer.
            var args = new Dictionary<string, object>();
            var peekCount = 0;
            var result = CheckExpressionMatches(plugin, plugin.Matches, args, peekCount, 0);
            result.Plugin = plugin;
            return result;
        }


        private MatchResult CheckMatchesForLexer(CompilerPlugin plugin, List<TokenMatch> matches, Dictionary<string, object> args, int peekCount, int matchCount)
        {
            return null;
        }


        private MatchResult CheckExpressionMatches(CompilerPlugin plugin, List<TokenMatch> matches, Dictionary<string, object> args, int peekCount, int matchCount)
        {
            var isMatch = true;
            var token = peekCount == 0 ? this.TokenIt.NextToken : this.TokenIt.Peek(peekCount);
            var totalMatched = matchCount;
            
            foreach (var match in matches)
            {
                var continueCheck = false;
                var trackNamedArgs = true;
                var valueMatched = false;

                // Termninators
                if (match.TokenType == "@exprTerminators" 
                    && ( Terminators.ExpFlexibleEnd.ContainsKey(token.Token) || Terminators.ExpThenEnd.ContainsKey(token.Token) ) 
                    )
                {
                    // Don't increment the peekcount
                    isMatch = totalMatched >= plugin.TotalRequiredMatches;
                    break;
                }
                // Check for ";" and EOF ( end of file/text )
                if (token.Token == Tokens.Semicolon || token.Token == Tokens.EndToken)
                {
                    isMatch = totalMatched >= plugin.TotalRequiredMatches;
                    break;
                }

                // Check 1: Group tokens ?
                if(match.IsGroup)
                {
                    var submatches = ((TokenGroup) match).Matches;
                    var result = CheckExpressionMatches(plugin, submatches, args, peekCount, totalMatched);
                    if(match.IsRequired && !result.Success)
                    {
                        isMatch = false;
                        break;
                    }
                    if(result.Success)
                    {
                        peekCount = result.TokenCount;
                        if(match.IsRequired)
                            totalMatched += result.TotalMatched;
                    }
                }
                // Check 2: starttoken?
                else if (match.TokenType == "@starttoken")
                {
                    continueCheck = true;
                    totalMatched++;
                }
                // Check 2a: tokenmap1
                else if (match.TokenType == "@tokenmap1")
                {
                    if (plugin.TokenMap1 == null || !plugin.TokenMap1.ContainsKey(token.Token.Text))
                    {
                        isMatch = false;
                        break;
                    }
                    continueCheck = true;
                    totalMatched++;
                }
                else if (match.TokenType == "@tokenmap2")
                {
                    if (plugin.TokenMap2 == null || !plugin.TokenMap2.ContainsKey(token.Token.Text))
                    {
                        isMatch = false;
                        break;
                    }
                    continueCheck = true;
                    totalMatched++;
                }
                // Check 2c: "identSymbol" must exist
                else if (match.TokenType == "@identsymbol")
                {
                    var symbolExists = this.Symbols.Contains(token.Token.Text);
                    continueCheck = symbolExists;
                    if (!continueCheck)
                    {
                        isMatch = false;
                        break;
                    }
                    totalMatched++;
                }
                // Check 2c: "identSymbol" must exist
                else if (match.TokenType == "@singularsymbol")
                {
                    var plural = token.Token.Text + "s";
                    var symbolExists = this.Symbols.Contains(plural);
                    continueCheck = symbolExists;
                    if (!continueCheck)
                    {
                        isMatch = false;
                        break;
                    }
                    totalMatched++;
                }
                // Check 2d: paramlist = @word ( , @word )* parameter names
                else if(match.TokenType == "@paramnames")
                {
                    var isvalidParamList = true;
                    var maxParams = 10;
                    var totalParams = 0;
                    var paramList = new List<object>();

                    while(totalParams <= maxParams)
                    {
                        var token2 = this.TokenIt.Peek(peekCount, false);
                        if(token2.Token == Tokens.Comma)
                        {
                            peekCount++;
                        }
                        else if(token2.Token.Kind == TokenKind.Ident)
                        {
                            paramList.Add(token2.Token.Text);
                            peekCount++;
                        }
                        else
                        {
                            peekCount--;
                            break;
                        }
                        totalParams++;
                    }
                    isMatch = isvalidParamList;
                    continueCheck = isMatch;
                    if (continueCheck)
                    {
                        trackNamedArgs = false;
                        if(!string.IsNullOrEmpty(match.Name))
                        {
                            args[match.Name] = token;
                            args[match.Name + "Value"] = new LArray(paramList);
                        }
                        totalMatched++;
                    }
                    else
                    {
                        break;
                    }
                }
                // Check 3a: Optional words with text
                else if (!match.IsRequired && match.Text != null && match.Text != token.Token.Text)
                {
                    continueCheck = false;
                }
                // Check 3b: Optional words matched
                else if (!match.IsRequired && match.IsMatchingValue(token.Token))
                {
                    continueCheck = true;
                }
                // Check 4: Optional word not matched
                else if (!match.IsRequired && !match.IsMatchingValue(token.Token))
                {
                    continueCheck = false;
                }
                // Check 5a: Expected word
                else if (match.IsRequired && match.TokenType == null && match.Text == token.Token.Text)
                {
                    continueCheck = true;
                    totalMatched++;
                }
                // Check 5b: Expected word in list
                else if (match.IsRequired && match.TokenType == null && match.Values != null)
                {
                    if (!match.IsMatchingValue(token.Token))
                    {
                        isMatch = false;
                        break;
                    }
                    continueCheck = true;
                    valueMatched = true;
                    totalMatched++;
                }
                // Check 6: check the type of n1
                else if (match.IsMatchingType(token.Token))
                {
                    continueCheck = true;
                    totalMatched++;
                }
                else
                {
                    isMatch = false;
                    break;
                }
                if (continueCheck)
                {
                    if (!string.IsNullOrEmpty(match.Name) && trackNamedArgs)
                    {
                        args[match.Name] = token; 
                        if(match.TokenPropEnabled)
                        {
                            // 1. figure out which token map to use.
                            var lookupmap = plugin.StartTokenMap;

                            if (match.TokenType == "@tokenmap1")
                                lookupmap = plugin.TokenMap1;
                            else if (match.TokenType == "@tokenmap2")
                                lookupmap = plugin.TokenMap2;

                            // Case 1: Start token replacement value
                            if (match.TokenPropValue == "value")
                            {
                                var startToken = token.Token.Text;
                                args[match.Name + "Value"] = lookupmap[startToken];
                            }
                            // Case 2: Token value
                            else if (match.TokenPropValue == "tvalue")
                            {
                                LObject val = LObjects.Null;
                                if (match.TokenType == "@number")
                                    val = new LNumber((double)token.Token.Value);
                                else if (match.TokenType == "@time")
                                    val = new LTime((TimeSpan)token.Token.Value);
                                else if (match.TokenType == "@word")
                                    val = new LString((string)token.Token.Value);
                                else if (match.TokenType == "@starttoken")
                                    val = new LString(token.Token.Text);
                                args[match.Name + "Value"] = val;
                            }
                            // Case 2: Token value
                            else if (match.TokenPropValue == "tvaluestring")
                            {
                                LObject val = LObjects.Null;
                                if (match.TokenType == "@number")
                                    val = new LString(((double)token.Token.Value).ToString(CultureInfo.InvariantCulture));
                                else if (match.TokenType == "@time")
                                    val = new LString(((TimeSpan)token.Token.Value).ToString());
                                else if (match.TokenType == "@starttoken")
                                    val = new LString(token.Token.Text);
                                else if (match.TokenType == "@word")
                                    val = new LString(token.Token.Text);
                                else if (match.TokenType == "@singularsymbol")
                                    val = new LString(token.Token.Text);
                                args[match.Name + "Value"] = val;
                            }
                        }
                        // matching values
                        else if(valueMatched)
                        {
                            args[match.Name + "Value"] = token.Token.Text;
                        }
                    }
                    // Matched: increment.
                    peekCount++;
                    token = this.TokenIt.Peek(peekCount, false);
                }
            }
            var res = new MatchResult(isMatch, null, args);
            res.TotalMatched = totalMatched;
            res.TokenCount = peekCount;
            return res;
        }


        private Expr ParseExpressionGrammar(Expr buildexpr, CompilerPlugin plugin, List<TokenMatch> matches, int peekCount)
        {
            var isMatch = true;
            var token = peekCount == 0 ? this.TokenIt.NextToken : this.TokenIt.Peek(peekCount);
            
            foreach (var match in matches)
            {
                if (match.TokenType == "@expr" && match.TokenPropValue == "block")
                {
                    if (match.Ref == "buildexpr")
                    {
                        var blockExp = buildexpr as IBlockExpr;
                        this.Parser.ParseBlock(blockExp);
                    }
                }

                // Matched: increment.
                peekCount++;
                token = this.TokenIt.Peek(peekCount, false);
            }
            return buildexpr;
        }


        class TokenFetcherParserLevel
        {
            private TokenIterator _tokenIt;


            public virtual TokenData Curr()
            {
                return this._tokenIt.NextToken;
            }


            public virtual TokenData Peek(int count)
            {
                return this._tokenIt.Peek(count);
            }
        }


        class TokenFetcherLexerLevel
        {
            private Lexer _lexer;


            public virtual TokenData Curr()
            {
                return this._lexer.NextToken();
            }


            public virtual TokenData Peek(int count)
            {
                return this._lexer.PeekToken();
            }
        }
    }



    public class MatchResult
    {
        public bool Success;
        public int TokenCount;
        public IDictionary<string, object> Args;
        public CompilerPlugin Plugin;
        public int TotalMatched;


        public MatchResult(bool success, CompilerPlugin plugin, IDictionary<string, object> args)
        {
            this.Args = args;
            this.Plugin = plugin;
            this.Success = success;
        }
    }
}
