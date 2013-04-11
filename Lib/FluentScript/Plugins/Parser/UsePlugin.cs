using System.IO;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Use plugin allows the reuse of other script files and/or system namespaces
    
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling boolean values in differnt formats (yes, Yes, no, No, off Off, on On).
    /// </summary>
    public class UseLexPlugin : LexPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public UseLexPlugin()
        {
            _tokens = new string[] { "use" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "use ( filepath | relative_file_path | environment_var_file_path | sys_namespace )"; }
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
                    @"use c:\scripts\tasks.fl",
                    @"use \scripts\tasks.fl",
                    @"use ..\scripts\tasks.fl",
                    @"use env.fshome\tasks.fl",
                    @"use sys.io"
                };
            }
        }


        /// <summary>
        /// Parse the use statement.
        /// </summary>
        /// <returns></returns>
        public override Token[] Parse()
        {
            var token = _lexer.ReadLine(false);
            var source = token.Text.ToLower();

            
            return null;
        }
    }



    /// <summary>
    /// Use plugin.
    /// </summary>
    public class UsePlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public UsePlugin()
        {
            this.StartTokens = new string[] { "use" };
        }


        /// <summary>
        /// Grammer for the plugin.
        /// </summary>
        public override string Grammer
        {
            get { return "use ( filepath | relative_file_path | environment_var_file_path | sys_namespace )"; }
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
                    @"use c:\scripts\tasks.fl",
                    @"use \scripts\tasks.fl",
                    @"use ..\scripts\tasks.fl",
                    @"use env.fshome\tasks.fl",
                    @"use sys.io"
                };
            }
        }


        /// <summary>
        /// Parses the using statement.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            return null;
        }
    }


    /// <summary>
    /// Class to execute a using statement.
    /// </summary>
    public class UseStmt : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public UseStmt()
        {
            IsImmediatelyExecutable = true;
        }


        /// <summary>
        /// The source path/namespace to use.
        /// </summary>
        public string Source { get; set; }


        /// <summary>
        /// Executes the using.
        /// </summary>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            var source = Source;

            // Case 1: System namespace
            if (source.StartsWith("sys"))
            {
                // TODO: Register the types found in the sys.namespace.            
            }
            // Case 2: Relative file path
            else if (source.StartsWith(".."))
            {
                // TODO: Get absolute path of file
            }
            // Case 3: Absolution file path from root
            else if (source.StartsWith("\\"))
            {
                // TODO: Get absolute path of file
            }
            // Case 4: Envrionment variable path
            else if (source.StartsWith("env"))
            {
                // TODO: Get absolute path of file
            }
            // Case 5: File path
            else
            {
                if (!File.Exists(source))
                    throw new FileNotFoundException("File : " + source + " does not exist");
            }

            var script = global::System.IO.File.ReadAllText(source);
            Context ctx = null;
            var scriptParser = new Lib.Parser.Parser(ctx);
            scriptParser.Parse(script);
            Lib.Parser.Parser parser = null;
            parser.Statements.AddRange(scriptParser.Statements);
            return LObjects.Null;
        }        
    }
}
