using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.MetaPlugins;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Uri plugin allows you urls and file paths without surrounding them in 
    // quotes as long as there are no spaces. These are interpreted as strings.
    
    var url1 = www.yahoo.com;
    var url2 = http://www.google.com;
    var url3 = http://www.yahoo.com?user=kishore%20&id=123;
    var file1 = c:\users\kishore\settings.ini;
    var file2 = c:/data/blogposts.xml;
    var printer = \\printnetwork1\printer1
    
    // Since this file has a space in it... you have to surround in quotes.
    var file3 = 'c:/data/blog posts.xml';
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling boolean values in differnt formats (yes, Yes, no, No, off Off, on On).
    /// </summary>
    public class PluginPlugin : ExprPlugin
    {
        //private string[] _requiredFieldsForExpr;
        //private string[] _requiredFieldsForTokens;


        /// <summary>
        /// Initialize
        /// </summary>
        public PluginPlugin()
        {
            this.StartTokens = new string[] { "compiler_plugin" };
            this.IsAutoMatched = true;
            this.IsStatement = true;
            this.IsCodeBlockSupported = true;
            //this._requiredFieldsForExpr = new string[] { "grammar_parse", "start_tokens", "parse" };
            //this._requiredFieldsForTokens = new string[] { "tokens" };
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.Advance();
            var name = _tokenIt.ExpectId(true, true);
            var expr = new PluginExpr();
            expr.Name = name;

            // 1. Push the new symbol scope
            this.Ctx.Symbols.Push(new SymbolsFunction(name), true);
            expr.SymScope = this.Ctx.Symbols.Current;

            // 2. Parse block
            _parser.ParseBlock(expr);

            // 3. Pop the last symbol scope.
            this.Ctx.Symbols.Pop();

            //this.Validate(expr);
            return expr;
        }
    }


    public class PluginExpr : BlockExpr
    {
        public PluginExpr()
        {
            this.IsImmediatelyExecutable = true;
        }


        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name;


        public override object DoEvaluate(IAstVisitor visitor)
        {
            LangHelper.Evaluate(this.Statements, this, visitor);
            this.SetupPlugin();
            return LObjects.Null;
        }


        private void SetupPlugin()
        {
            // 1. Create the meta plugin
            var plugin = new CompilerPlugin();
            plugin.Name = this.Name;
            
            // 2. Load default properties such as desc, company, etc.
            this.LoadDefaultProperties(plugin);

            // 3. Load the examples
            this.LoadExamples(plugin);

            // 4. token replacements ? or expression plugin?
            if(plugin.PluginType == "expr")
            {
                plugin.TokenMap1 = this.GetTokenMap("tokenmap1");
                plugin.TokenMap2 = this.GetTokenMap("tokenmap2");
                this.LoadStartTokensAsMap(plugin);                
            }
            else if(plugin.PluginType == "token")
            {
                this.LoadStartTokensAsList(plugin);
                this.LoadTokenReplacements(plugin);                
            }
            else if (plugin.PluginType == "lexer")
            {
                this.LoadStartTokensAsList(plugin);
            }

            // 5. Load the grammar and build functions.
            this.LoadGrammar(plugin);                
            if (this.Ctx.Memory.Contains("build"))
                plugin.BuildExpr = this.GetFunc("build");
            
            // 5. Finally register the plugin.
            this.Ctx.PluginsMeta.Register(plugin);
        }


        private void LoadDefaultProperties(CompilerPlugin plugin)
        {
            // 2. Assign all plugin properties
            plugin.Desc = this.GetOrDefaultString("desc", string.Empty);
            plugin.PluginType = this.GetOrDefaultString("type", "expr");
            plugin.Author = this.GetOrDefaultString("author", "Kishore Reddy");
            plugin.Company = this.GetOrDefaultString("company", "CodeHelix Solutions Inc.");
            plugin.Url = this.GetOrDefaultString("url", "http://www.codehelixsolutions.com");
            plugin.Url2 = this.GetOrDefaultString("url2", "http://fluentscript.codeplex.com");
            plugin.Doc = this.GetOrDefaultString("doc", "http://fluentscript.codeplex.com/documentation");
            plugin.Version = this.GetOrDefaultString("version", "0.9.8.10");
            plugin.IsStatement = this.GetOrDefaultBool("isStatement", false);
            plugin.IsEndOfStatementRequired = this.GetOrDefaultBool("isEOSRequired", false);
            plugin.IsEnabled = this.GetOrDefaultBool("isEnabled", true);
            plugin.IsSystemLevel = this.GetOrDefaultBool("isSystemLevel", false);
            plugin.IsAutoMatched = this.GetOrDefaultBool("isAutoMatched", false);
            plugin.IsAssignmentSupported = this.GetOrDefaultBool("isAssignmentSupported", false);
            if (this.Ctx.Memory.Contains("defaults"))
            {
                var map = this.Ctx.Memory["defaults"] as LMap;
                plugin.ParseDefaults = map.Value;
            }
        }


        private void LoadExamples(CompilerPlugin plugin)
        {
            // 3. Examples
            var examplesList = this.Ctx.Memory.Get<object>("examples") as LArray;
            if (examplesList != null && examplesList.Value != null && examplesList.Value.Count > 0)
            {
                var examples = new List<string>();
                foreach (var lobj in examplesList.Value)
                {
                    var example = lobj as LObject;
                    var exampleText = example.GetValue().ToString();
                    examples.Add(exampleText);
                }
                plugin.Examples = examples.ToArray();
            }
        }


        private void LoadStartTokensAsMap(CompilerPlugin plugin)
        {
            // 4. Setup the start tokens.
            var tokens = new List<string>();
            var stokenMap = this.Ctx.Memory.Get<object>("start_tokens") as LObject;

            if (stokenMap.Type == LTypes.Array)
            {
                var list = stokenMap as LArray;

                // 5. Not start tokens supplied ?
                if (list != null && list.Value.Count != 0)
                {
                    plugin.StartTokenMap = new Dictionary<string, object>();
                    for (var ndx = 0; ndx < list.Value.Count; ndx++)
                    {
                        var val = list.Value[ndx] as LString;
                        tokens.Add(val.Value);
                        plugin.StartTokenMap[val.Value] = val;
                    }
                    plugin.StartTokens = tokens.ToArray();
                }
            }
            else if (stokenMap.Type == LTypes.Map)
            {
                var map = stokenMap as LMap;

                // 5. Start tokens supplied ?
                if (map != null && map.Value.Count != 0)
                {
                    plugin.StartTokenMap = map.Value;
                    foreach (var keyval in map.Value)
                    {
                        tokens.Add(keyval.Key);
                    }
                    plugin.StartTokens = tokens.ToArray();
                }
            }
        }


        private void LoadStartTokensAsList(CompilerPlugin plugin)
        {
            if (!this.Ctx.Memory.Contains("start_tokens"))
                return;

            var list = this.Ctx.Memory.Get<object>("start_tokens") as LArray;
            List<string> tokens = null;

            // 5. Not start tokens supplied ?
            if (list != null && list.Value.Count != 0)
            {
                tokens = new List<string>();
                for (var ndx = 0; ndx < list.Value.Count; ndx++)
                {
                    var val = list.Value[ndx] as LString;
                    tokens.Add(val.Value);
                }
            }
            if(tokens != null && tokens.Count > 0)
                plugin.StartTokens = tokens.ToArray();
        }


        private void LoadGrammar(CompilerPlugin plugin)
        {
            // 6. Parse the grammar
            plugin.Grammar = this.GetOrDefaultString("grammar_parse", "");

            var parser = new GrammerParser();            
                
            // 7. Parse grammar match ( if present )
            plugin.GrammarMatch = this.GetOrDefaultString("grammar_match", "");
            if (!string.IsNullOrEmpty(plugin.GrammarMatch))
            {
                plugin.Matches = parser.Parse(plugin.GrammarMatch);
                plugin.TotalRequiredMatches = parser.TotalRequired(plugin.Matches);
            }
            
            // 7a. check for empty
            if (!string.IsNullOrEmpty(plugin.Grammar))
            {
                plugin.MatchesForParse = parser.Parse(plugin.Grammar);                
            }            
        }


        private void LoadTokenReplacements(CompilerPlugin plugin)
        {
            if (!this.Ctx.Memory.Contains("tokens"))
                return;

            var array = this.Ctx.Memory.Get<object>("tokens") as LArray;
            var records = array.Value;
            var replacements = new List<string[]>();
            foreach (var record in records)
            {
                var list = record as LArray;
                if (list != null)
                {
                    var columns = list.Value;
                    var alias = columns[0] as LString;
                    var replacement = columns[1] as LString;
                    if (alias != null && replacement != null)
                    {
                        replacements.Add(new string[2] { alias.Value, replacement.Value });
                    }
                }
            }
            plugin.TokenReplacements = replacements;
        }


        private void PrintFieldValues()
        {
            var names = this.SymScope.GetSymbolNames();
            for(var ndx = 0; ndx < names.Count; ndx++)
            {
                var name = names[ndx];
                var obj = this.Ctx.Memory.Get<object>(name);
                Console.WriteLine(name + ":" + ((LObject)obj).GetValue().ToString());
            }
        }


        private string GetOrDefaultString(string key, string defaultVal)
        {
            if (!this.Ctx.Memory.Contains(key))
                return defaultVal;

            var val = this.Ctx.Memory.Get<object>(key) as LString;
            return val.Value;
        }


        private Expr GetFunc(string key)
        {
            if (!this.Ctx.Memory.Contains(key))
                return null;

            var val = this.Ctx.Memory.Get<object>(key);
            return ((LFunction)val).Value as Expr;
        }


        private bool GetOrDefaultBool(string key, bool defaultVal)
        {
            if (!this.Ctx.Memory.Contains(key))
                return defaultVal;

            var val = this.Ctx.Memory.Get<object>(key) as LBool;
            return val.Value;
        }


        private IDictionary<string, object> GetTokenMap(string key)
        {
            if (!this.Ctx.Memory.Contains(key))
                return null;

            var map = this.Ctx.Memory.Get<object>(key) as LMap;

            if (map != null && map.Value.Count != 0)
            {
                return map.Value;
            }
            return null;
        }
    }
}
