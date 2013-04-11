using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser;
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
    // Alias plugin allows setting up alias of words to other words/tokens in a script
    
    alias def to function
    alias set to var
    
    // After the alias configured
    set result1 = 1
    var result2 = 2
    
    </doc:example>
    ***************************************************************************/
    // <fs:plugin-autogenerate>
    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class AliasPlugin : ExprPlugin, IParserCallbacks
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public AliasPlugin()
        {
            this.StartTokens = new string[] { "alias" };
            this.IsStatement = true; 
            this.IsEndOfStatementRequired = true;
            this.IsAutoMatched = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "alias <ident> to <ident>";
            }
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
                    "alias set to var",
                    "alias def to function"
                };
            }
        }
        // </fs:plugin-autogenerate>


        /// <summary>
        /// Parses the alias statement.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.ExpectIdText("alias");
            var aliasName = _tokenIt.ExpectId(true, true);
            _tokenIt.ExpectIdText("to");
            var actualName = _tokenIt.ExpectId(true, true);
            Token actual = null;
            if (Tokens.AllTokens.ContainsKey(actualName))
                actual = Tokens.AllTokens[actualName];
            else
                actual = TokenBuilder.ToIdentifier(actualName);
            
            var stmt = new AliasStmt() { Alias = aliasName, Actual = actual };
            return stmt;
        }


        /// <summary>
        /// After parsing is complete, register the alias.
        /// </summary>
        /// <param name="node"></param>
        public override void OnParseComplete(AstNode node)
        {
            var stmt = node as AliasStmt;
            //var plugin = new AliasTokenPlugin(stmt.Alias, stmt.Actual);
            //plugin.Init(_parser, _tokenIt);
            var plugin = new CompilerPlugin();
            plugin.PluginType = "token";
            plugin.TokenReplacements = new List<string[]>();
            plugin.TokenReplacements.Add(new string[] { stmt.Alias, stmt.Actual.Text });
            plugin.Precedence = 1;
            plugin.IsEnabled = true;
            Ctx.PluginsMeta.Register(plugin);
        }
    }



    /// <summary>
    /// Statement that assigns one token as an alias to another.
    /// </summary>
    public class AliasStmt : Expr
    {
        /// <summary>
        /// Initailize
        /// </summary>
        public AliasStmt()
        {
            IsImmediatelyExecutable = true;
        }
        

        /// <summary>
        /// The alias to setup
        /// </summary>
        public string Alias { get; set; }


        /// <summary>
        /// The mapping of the alias.
        /// </summary>
        public Token Actual { get; set; }


        /// <summary>
        /// Executes the statement.
        /// </summary>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            return LObjects.Null;
            // This is executed during parse phase.
            // But this statement exists so it can be represented in the AST.

            // Not sure about the best design pattern for this.
            // 1. Should this registration occur in the AliasPlugin or here?
            // 2. Should the parser check for IsImmediatelyExecutable and execute it ?
            // Ctx.Plugins.RegisterTokenPlugin(new AliasTokenPlugin(Alias, Actual), true);
        }
    }
}
