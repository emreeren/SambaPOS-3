using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Parsing;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.Plugins
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
    public class ExecPlugin : CustomFunctionPluginBase
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public ExecPlugin() 
        {
            this.Init("exec");
            _funcMeta = new FunctionMetaData("exec", null);
            _funcMeta.AddArg("program",    "string",  true,  "",   string.Empty, @"c:\tools\nunit\nunit.exe", "program to launch");
            _funcMeta.AddArg("workingdir", "string", false, "in", string.Empty, @"c:\tools\nunit\", "working directory to launch in");
            _funcMeta.AddArg("args",       "list",   false, "",   string.Empty, "", "arguments to the program");
            _funcMeta.AddArg("failOnError","bool",   false, "", false, "", "arguments to the program");
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
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var expr = new ExecExpr(_funcMeta);
            base.ParseFunction(expr);

            if (expr.ParamListExpressions.Count == 0)
                throw _tokenIt.BuildSyntaxExpectedException("exec plugin requires at least 1 parameter");
            return expr;
        }
    }



    /// <summary>
    /// Variable expression data
    /// </summary>
    public class ExecExpr : ParameterExpr
    {
        /// <summary>
        /// Metadata about the function.
        /// </summary>
        /// <param name="meta"></param>
        public ExecExpr(FunctionMetaData meta)
        {
            Init(meta);
        }


        /// <summary>
        /// Evaluate
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            var exePath = "";
            var workingDir = "";
            var failOnError = false;
            LArray args = null;
            var exitcode = -1;
            try
            {
                this.ResolveParams();
                exePath = this.GetParamValueString(0, false, string.Empty);
                workingDir = this.GetParamValueString(1, true, string.Empty);
                args = this.GetParamValue(2, true, null) as LArray;
                failOnError = this.GetParamValueBool(3, true, false);

                // Convert the items in the array to strings.
                // TODO: type-changes
                //var list = args.Raw;
                var list = new List<object>();
                var stringArgs = "";
                if (args != null && args.Value != null)
                {
                    foreach (var item in args.Value)
                    {
                        var lobj = (LObject)item;
                        stringArgs += Convert.ToString(lobj.GetValue()) + " ";
                    }
                }
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = stringArgs;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = workingDir;                
                p.Start();
                // TODO: Set up options on this plugin to configure wait time ( indefinite | milliseconds )
                p.WaitForExit();
                exitcode = p.ExitCode;
            }
            catch (Exception ex)
            {
                exitcode = 1;
                if (failOnError)
                { 
                    var error = string.Format("An error occurred executing external application '{0}', in '{1}', with '{2}'.\r\n"
                              + "message: {3}", exePath, workingDir, args, ex.Message);
                    throw new LangFailException(error, this.Ref.ScriptName, this.Ref.Line);
                }
            }
            return new LNumber(Convert.ToDouble(exitcode));
        }
    }
}
