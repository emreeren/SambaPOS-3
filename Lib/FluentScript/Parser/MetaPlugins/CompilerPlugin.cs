using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;

namespace Fluentscript.Lib.Parser.MetaPlugins
{
    /// <summary>
    /// Represents a compiler plugin developed in javascript/fluentscript ( bootstrapped plugin ).
    /// </summary>
    public class CompilerPlugin
    {
        public List<TokenMatch> Matches;
        public List<TokenMatch> MatchesForParse;

        public CompilerPlugin()
        {            
        }


        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name;


        /// <summary>
        /// Full name of the plugin.
        /// </summary>
        public string FullName;


        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Desc;


        /// <summary>
        /// The type of the plugin ( expr | token )
        /// </summary>
        public string PluginType;


        /// <summary>
        /// author of the plugin.
        /// </summary>
        public string Author;


        /// <summary>
        /// Company of the plugin.
        /// </summary>
        public string Company;


        /// <summary>
        /// Ulr for more info about the plugin
        /// </summary>
        public string Url;


        /// <summary>
        /// Url for more info about the plugin
        /// </summary>
        public string Url2;
        

        /// <summary>
        /// Documentation link for the plugin
        /// </summary>
        public string Doc;


        /// <summary>
        /// License for the plugin
        /// </summary>
        public string License;


        /// <summary>
        /// Version of the plugin
        /// </summary>
        public string Version;

        /// <summary>
        /// Used for ordering of plugins.
        /// </summary>
        public int Precedence { get; set; }


        /// <summary>
        /// Examples of grammer
        /// </summary>
        public string[] Examples { get; set; }


        /// <summary>
        /// Whether or not this combinator can be made into a statement.
        /// </summary>
        public bool IsStatement { get; set; }


        /// <summary>
        /// Whether or not this is a system level plugin.
        /// </summary>
        public bool IsSystemLevel { get; set; }


        /// <summary>
        /// Whether or not assignment is supported by this plugin.
        /// </summary>
        public bool IsAssignmentSupported { get; set; }


        /// <summary>
        /// Whether or not a termninator is supported
        /// </summary>
        public bool IsEndOfStatementRequired { get; set; }


        /// <summary>
        /// Whether or not this is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }


        /// <summary>
        /// Whether or not the plugin auto handles if start tokens match.
        /// </summary>
        public bool IsAutoMatched { get; set; }


        /// <summary>
        /// Grammar for matching the plugin.
        /// </summary>
        public string GrammarMatch { get; set; }


        /// <summary>
        /// The tokens starting the expression.
        /// </summary>
        public string[] StartTokens { get; set; }


        /// <summary>
        /// The token replacements.
        /// </summary>
        public List<string[]> TokenReplacements { get; set; }


        /// <summary>
        /// The start token map which maps start tokens to their values.
        /// </summary>
        public IDictionary<string, object> StartTokenMap { get; set; }


        /// <summary>
        /// A token map for representing another set of allowed tokens not part of the start tokens.
        /// </summary>
        public IDictionary<string, object> TokenMap1 { get; set; }


        /// <summary>
        /// The secondary token map for representing another set of allowed tokens
        /// </summary>
        public IDictionary<string, object> TokenMap2 { get; set; }


        /// <summary>
        /// Default arguments for parse method
        /// </summary>
        public IDictionary<string, object> ParseDefaults { get; set; }


        /// <summary>
        /// Expression to parse
        /// </summary>
        public Expr BuildExpr;

        
        /// <summary>
        /// The grammar representing the parse
        /// </summary>
        public string Grammar { get; set; }


        /// <summary>
        /// The total # of required matches - calculated based on matches.
        /// </summary>
        public int TotalRequiredMatches;


        public void AddStartTokens(params string[] tokens)
        {
            this.StartTokens = tokens;
        }


        public string GetFullGrammar()
        {
            var grammar = "";
            if (string.IsNullOrEmpty(this.Grammar))
                grammar = this.GrammarMatch;
            else
            {
                var ndxRef = this.Grammar.IndexOf("#grammarmatch");
                if (ndxRef >= 0)
                    grammar = this.GrammarMatch + " " + this.Grammar.Substring(ndxRef + 13);
            }
            return grammar;
        }


        //public void AddMatch(TokenMatch m)
        //{
        //    this.Matches.Add(m);
        //}


        /// <summary>
        /// For Internal use only: Generic object to hold either a tokenreplacement class instance.
        /// </summary>
        public object Handler;
    }
}
