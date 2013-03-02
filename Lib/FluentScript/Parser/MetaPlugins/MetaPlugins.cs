
using System;
using System.Collections.Generic;
using ComLib.Lang.AST;
using ComLib.Lang.Core;
using ComLib.Lang.Helpers;
using ComLib.Lang.Plugins;
using ComLib.Lang.Types;


namespace ComLib.Lang.Parsing.MetaPlugins
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
        private List<CompilerPlugin> _allPlugins;
 
        private MatchResult _matchResult;
        private MatchResult _matchTokenResult;
        public MatchResult EmtpyMatch = new MatchResult(false, null, null);


        public MetaPluginContainer()
        {
            _pluginExprs = new Dictionary<string, List<CompilerPlugin>>();
            _pluginTokens = new Dictionary<string, List<CompilerPlugin>>();
            _allPlugins = new List<CompilerPlugin>();
        }


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

            if(plugin.ParseExpr == null)
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

            // Start token exists to trigger plugin check?
            if (!_pluginExprs.ContainsKey(token.Text))
                return false;

            _matchResult = EmtpyMatch;
            _matchResult = IsMatch();
            return _matchResult.Success;
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
                if (_pluginTokens.ContainsKey("$DateToken"))
                    plugins = _pluginTokens["$DateToken"];
            }
            if (plugins == null)
                return false;

            _matchTokenResult = new MatchResult(false, null, null);
            foreach (var plugin in plugins)
            {
                if (string.IsNullOrEmpty(plugin.Grammar))
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
                    var result = IsGrammarMatch(plugin);
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
        public Expr Parse(IAstVisitor visitor)
        {
            var expr = _matchResult.Plugin.ParseExpr as FunctionExpr;
            var ctx = expr.Ctx;
            var result = FunctionHelper.CallFunctionViaCSharpUsingLambda(ctx, expr, true, _matchResult.Args);
            var rexpr = ((LClass) result).Value as Expr;

            // Advance the tokens.
            this.TokenIt.Advance(_matchResult.TokenCount);
            return rexpr;
        }


        /// <summary>
        /// Visit expression.
        /// </summary>
        /// <returns></returns>
        public Token ParseTokens()
        {
            var plugin = _matchTokenResult.Plugin;
            if(!string.IsNullOrEmpty(plugin.Grammar))
            {
                var ctx = plugin.ParseExpr.Ctx;
                var expr = plugin.ParseExpr as FunctionExpr;
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
                result = IsGrammarMatch(plugin);
                if (result.Success)
                {
                    break;
                }
            }
            return result;
        }


        private MatchResult IsGrammarMatch(CompilerPlugin plugin)
        {
            // 5. Check Grammer.
            var args = new Dictionary<string, object>();
            var peekCount = 0;
            var result = CheckMatches(plugin, plugin.Matches, args, peekCount);
            result.Plugin = plugin;
            return result;
        }


        private MatchResult CheckMatches(CompilerPlugin plugin, List<TokenMatch> matches, Dictionary<string, object> args, int peekCount)
        {
            var isMatch = true;
            var token = peekCount == 0 ? this.TokenIt.NextToken : this.TokenIt.Peek(peekCount);
            foreach (var match in matches)
            {
                var incrementPeek = false;
                
                // Check 1: Group tokens ?
                if(match.IsGroup)
                {
                    var submatches = ((TokenGroup) match).Matches;
                    var result = CheckMatches(plugin, submatches, args, peekCount);
                    if(match.IsRequired && !result.Success)
                    {
                        isMatch = false;
                        break;
                    }
                }
                // Check 2: starttoken?
                else if (match.TokenType == "@starttoken")
                {
                    incrementPeek = true;
                }
                // Check 3a: Optional words with text
                else if (!match.IsRequired && match.Text != null && match.Text != token.Token.Text)
                {
                    incrementPeek = false;
                }
                // Check 3b: Optional words matched
                else if (!match.IsRequired && match.IsMatchingValue(token.Token))
                {
                    incrementPeek = true;
                }
                // Check 4: Optional word not matched
                else if (!match.IsRequired && !match.IsMatchingValue(token.Token))
                {
                    incrementPeek = false;
                }
                // Check 5: Expected word
                else if (match.IsRequired && match.TokenType == null && match.Text == token.Token.Text)
                {
                    incrementPeek = true;
                }
                // Check 6: check the type of n1
                else if (match.IsMatchingType(token.Token))
                {
                    incrementPeek = true;
                }
                else
                {
                    isMatch = false;
                    break;
                }
                if (incrementPeek)
                {
                    if (!string.IsNullOrEmpty(match.Name))
                    {
                        args[match.Name] = token; 
                        if(match.TokenPropEnabled && match.TokenPropValue == "value")
                        {
                            var startToken = token.Token.Text;
                            args[match.Name + "-value"] = plugin.StartTokenMap[startToken];
                        }
                    }
                    // Matched: increment.
                    peekCount++;
                    token = this.TokenIt.Peek(peekCount);
                }
            }
            var res = new MatchResult(isMatch, null, args);
            res.TokenCount = peekCount;
            return res;
        }
    }



    public class MatchResult
    {
        public bool Success;
        public int TokenCount;
        public IDictionary<string, object> Args;
        public CompilerPlugin Plugin;

        public MatchResult(bool success, CompilerPlugin plugin, IDictionary<string, object> args)
        {
            this.Args = args;
            this.Plugin = plugin;
            this.Success = success;
        }
    }
}
