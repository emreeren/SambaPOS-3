using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
{

    /* *************************************************************************
    <doc:example>	
    // Exec plugin allows launching/execution of external programs.
    // lowercase and uppercase days are supported:
    // 1. Monday - Sunday
    // 2. monday - sunday
    // 3. today, tomorrow, yesterday
    
    var day = Monday;
    var date = tomorrow at 3:30 pm;
    
    if tommorrow is Saturday then
	    print Thank god it's Friday
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class ExecPlugin : ExprPlugin, ISetupPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public ExecPlugin()
        {
            this.StartTokens = new string[] { "exec" };
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
                return "'exec' <function_params>";
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
                    "exec msbuildhome\\msbuild.exe",
                    "exec msbuildhome\\msbuild.exe in: 'c:\\myapp\\build'",
                    "exec msbuildhome\\msbuild.exe in: 'c:\\myapp\\build' [ 'arg-a', 'arg-b', 'arg-c' ]"
                };
            }
        }


        /// <summary>
        /// Setup the exec plugin.
        /// </summary>
        /// <param name="ctx"></param>
        public void Setup(Context ctx)
        {
            var func = new FunctionExpr("exec", null);
            func.Meta.AddArg( "program",    "program to launch",              "",   "string", true,  string.Empty, @"c:\tools\nunit\nunit.exe");
            func.Meta.AddArg( "workingdir", "working directory to launch in", "in", "string", false, string.Empty, @"c:\tools\nunit\");
		    func.Meta.AddArg( "args",       "arguments to the program",       "",   "list",   false, string.Empty, "");            
            ctx.Functions.Register("exec", func);
        }        


        /// <summary>
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.ExpectIdText("exec");
            var execExpr = new ExecExpr();            
            var expr = _parser.ParseFuncExpression(null);
            return null;
        }
    }



    /// <summary>
    /// Variable expression data
    /// </summary>
    public class ExecExpr : Expr
    {
        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            var exePath = "";
            var workingDir = "";
            var failOnError = false;            
            var args = "";

            try
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = args;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = workingDir;
                p.Start();
            }
            catch (Exception ex)
            {
                if (failOnError)
                { 
                    var error = string.Format("An error occurred executing external application '{0}', in '{1}', with '{2}'.\r\n"
                              + "message: {3}", exePath, workingDir, args, ex.Message);
                    throw BuildRunTimeException(error);
                }
            }
            return LNull.Instance;
        }
    }
}
