using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Helpers;
using ComLib.Lang.Parsing;
using ComLib.Lang.Parsing.MetaPlugins;
using ComLib.Lang.Types;
using ComLib.Lang.Phases;
using ComLib.Lang.Runtime;
// </lang:using>

namespace ComLib.Lang
{
    /// <summary>
    /// Light version of javascript with some "sandbox" features coming up.
    /// </summary>
    /// <remarks>
    /// Provides high-level functionality for parsing/executing scripts.
    /// 
    /// Features include:
    /// 1. Convert script into a list of tokens ( Using Lexer ) - prints out line numbers and char positions of each token
    /// 2. Convert script into a sequence of expressions/statements (Using Parser ) - prints out line numbers and char positions of exp/stmts.
    /// 3. Only parse without executing
    /// 4. Parse and execute.
    /// 5. Provides benchmark capabilities of executing each statement.
    /// </remarks>
    public class Interpreter
    {
        //private InterpreterSettings _settings;
        private Memory _memory;
        private Parser _parser;
        private Context _context;
        private LangSettings _settings;
        private RunResult _runResult;
        private PhaseContext _phaseCtx;
        private bool _pluginsInitialized;
        private int _lastInitializationPluginCount = 0;


        /// <summary>
        /// Initialize
        /// </summary>
        public Interpreter()
        {            
            _settings = new LangSettings();
            
            // Initialzie the context.
            _context = new Context();
            _context.Settings = _settings;
            _context.Limits.Init();

            _memory = _context.Memory;
            _parser = new Parser(_context);
            _parser.Settings = _settings;
            InitSystemFunctions();
        }


        /// <summary>
        /// Scope of the script
        /// </summary>
        public Memory Memory
        {
            get { return _context.Memory; }
        }


        /// <summary>
        /// Context for the script.
        /// </summary>
        public Context Context
        {
            get { return _context; }
        }


        /// <summary>
        /// The settings for the interpreter.
        /// </summary>
        public LangSettings Settings
        {
            get { return _context.Settings; }
        }


        /// <summary>
        /// Run result
        /// </summary>
        public RunResult Result
        {
            get { return _runResult; }
        }


        /// <summary>
        /// Registers all the plugins.
        /// </summary>
        public void RegisterAllPlugins()
        {
            this.Context.Plugins.RegisterAll();
            this.RegisterMetaPlugins();
        }


        /// <summary>
        /// Load all the meta plugins from the plugins folders.
        /// </summary>
        public void RegisterMetaPlugins()
        {
            if(!Directory.Exists(this.Settings.PluginsFolder))
                throw new DirectoryNotFoundException("Directory for meta plugins : " + this.Settings.PluginsFolder + " does not exist");

            var files = Directory.GetFiles(this.Settings.PluginsFolder);
            foreach(var file in files)
            {
                this.RegisterMetaPlugin(file);
            }
        }


        /// <summary>
        /// Load the meta plugin with the file specified.
        /// </summary>
        /// <param name="file"></param>
        public void RegisterMetaPlugin(string file)
        {
            var currentMetaPlugin = "";
            try
            {
                var fileInfo = new FileInfo(file);
                
                // 1. store the current meta plugin being loaded ( in -case error loading we know which one )
                currentMetaPlugin = fileInfo.Name;
                
                // 2. only load individual plugins with naming convention "plugin-<name>.js"
                //    other plugin file such as "plugins.js" has all plugins.
                var enableFilter = true;
                if (!enableFilter || ( enableFilter && fileInfo.Name.Contains("-")))
                {
                    var script = File.ReadAllText(file);
                    this.Execute(script);
                }
            }
            catch (Exception ex)
            {
                var error = "Error loading meta plugin : " + currentMetaPlugin + " " + ex.Message;
                throw new InvalidOperationException("Configuration Error: " + error);
            }
        }
        

        /// <summary>
        /// Register the callback for custom functions
        /// </summary>
        /// <param name="funcCallPattern">Pattern for the function e.g. "CreateUser", or "Blog.*"</param>
        /// <param name="callback">The callback to call</param>
        public void SetFunctionCallback(string funcCallPattern, Func<string ,string, FunctionCallExpr, object> callback)
        {
            _parser.Context.ExternalFunctions.Register(funcCallPattern, callback);
        }


        /// <summary>
        /// Parses the script but does not execute it.
        /// </summary>
        /// <param name="scriptPath">Path to the script</param>
        public void ParseFile(string scriptPath)
        {
            var script = ReadFile(scriptPath);
            Parse(script);
        }


        /// <summary>
        /// Parses the script but does not execute it.
        /// </summary>
        /// <param name="script"></param>
        public void Parse(string script)
        {
            this.Execute(script, true, true, new ParsePhase(_parser), new ShutdownPhase());
        }


        /// <summary>
        /// Parses the script but does not execute it.
        /// </summary>
        /// <param name="scriptPath">Path to the script</param>
        public void LintFile(string scriptPath)
        {
            var script = ReadFile(scriptPath);
            Lint(script);
        }


        /// <summary>
        /// Parses the script but does not execute it.
        /// </summary>
        /// <param name="script"></param>
        public void Lint(string script)
        {
            this.Execute(script, true, true, new ParsePhase(_parser), new LintPhase(true), new ShutdownPhase());
        }


        /// <summary>
        /// Executes the file.
        /// </summary>
        /// <param name="scriptPath">Path to the script</param>
        public void ExecuteFile(string scriptPath)
        {
            var script = ReadFile(scriptPath);
            Execute(script);
        }


        /// <summary>
        /// Executes the script
        /// </summary>
        /// <param name="script">Script text</param>
        public void Execute(string script)
        {
            this.Execute(script, true, true, new ParsePhase(_parser), new ExecutionPhase(true), new ShutdownPhase());
        }


        /// <summary>
        /// Executes existing already parsed.
        /// </summary>
        public void Execute()
        {
            this.Execute(string.Empty, false, false, new ExecutionPhase(true));
        }


        /// <summary>
        /// Runs this instance of the interpreter in interactive mode.
        /// </summary>
        public void Interactive()
        {
            this.InitPlugins();

            // 1. read line of code from console.
            var script = Console.ReadLine();
            script = script.Trim();
            if (string.Compare(script, "exit", StringComparison.InvariantCultureIgnoreCase) == 0)
                return;

            this.Execute(script);
            
            // 2. Check success of line
            if (!this._runResult.Success)
                return;

            while (true)
            {
                // Now keep looping
                // 3. Read successive lines of code and append
                script = Console.ReadLine();

                // 4. Check for exit flag.
                if (   string.Compare(script, "exit", StringComparison.InvariantCultureIgnoreCase) == 0
                    || string.Compare(script, "Exit", StringComparison.InvariantCultureIgnoreCase) == 0
                    || string.Compare(script, "EXIT", StringComparison.InvariantCultureIgnoreCase) == 0 )
                    break;

                // 5. Only process if not empty
                if (!string.IsNullOrEmpty(script))
                {
                    this.AppendExecute(script);

                    // 6. if error break;
                    if (!_runResult.Success)
                        break;
                }
            }
        }


        /// <summary>
        /// Executes the script
        /// </summary>
        /// <param name="script">Script text</param>
        /// <param name="target">The target language to translate the code to.</param>
        public void Translate(string script, string target)
        {
            this.Execute(script, true, true, new ParsePhase(_parser), new TranslateToJsPhase(), new ShutdownPhase());
        }


        /// <summary>
        /// Appends the script to the existing parsed scripts
        /// </summary>
        /// <param name="script"></param>
        public void Append(string script)
        {
            this.Execute(script, false, true, new ParsePhase(_parser));
        }


        /// <summary>
        /// Append the script to the existing code and executes only the new code.
        /// </summary>
        /// <param name="script"></param>
        public void AppendExecute(string script)
        {
            this.Execute(script, false, true, new ParsePhase(_parser), new ExecutionPhase(false));
        }


        /// <summary>
        /// Executes the script
        /// </summary>
        /// <param name="script">Script text</param>
        /// <param name="clearExistingCode">Whether or not to clear existing parsed code and start fresh.</param>
        public void Execute(string script, bool clearExistingCode, bool resetScript, params IPhase[] phases)
        {
            this.InitPlugins();
            if(_parser != null)
            {
                var execution = new Execution();
                execution.Ctx = _context;
                EvalHelper.Ctx = _context;
                _parser.OnDemandEvaluator = execution;
            }
            var phaseExecutor = new PhaseExecutor();

            // 1. Create the execution phase
            if (clearExistingCode)
            {
                _phaseCtx = new PhaseContext();
                _phaseCtx.Ctx = _context;
            }
            if (resetScript)
                _phaseCtx.ScriptText = script;

            var phasesList = phases.ToList();
            var result = phaseExecutor.Execute(script, _phaseCtx, _context, phasesList);
            this._runResult = result.Result;
        }


        /// <summary>
        /// Call a fluent script function from c#.
        /// </summary>
        /// <param name="functionName">The name of the function to call</param>
        /// <param name="convertApplicableTypes">Whether or not to convert applicable c# types to fluentscript types, eg. ints and longs to double, List(object) to LArrayType and Dictionary(string, object) to LMapType</param>
        /// <param name="args"></param>
        public object Call(string functionName, bool convertApplicableTypes, params object[] args)
        {
            return FunctionHelper.CallFunctionViaCSharp(this._context, functionName, convertApplicableTypes, args);
        }


        /// <summary>
        /// Replaces a token with another token.
        /// </summary>
        /// <param name="text">The text to replace</param>
        /// <param name="newValue">The replacement text</param>
        public void LexReplace(string text, string newValue)
        {
            _parser.Lexer.SetReplacement(text, newValue);
        }


        /// <summary>
        /// Removes a token during the lexing process.
        /// </summary>
        /// <param name="text">The text to remove</param>
        public void LexRemove(string text)
        {
            _parser.Lexer.SetRemoval(text);
        }


        /// <summary>
        /// Adds a token during the lexing process.
        /// </summary>
        /// <param name="before">whether to insert before or after</param>
        /// <param name="text">The text to check for inserting before/after</param>
        /// <param name="newValue">The new value to insert before/after</param>
        public void LexInsert(bool before, string text, string newValue)
        {
            _parser.Lexer.SetInsert(before, text, newValue);
        }


        /// <summary>
        /// Convert the script to a series of tokens.
        /// </summary>
        /// <param name="script">The script content or file name</param>
        /// <param name="isFile">Whether or not the script supplied is a filename or actual script content</param>
        /// <returns></returns>
        public List<TokenData> ToTokens(string script, bool isFile)
        {
            List<TokenData> tokens = null;
            if (isFile)
            {
                script = File.ReadAllText(script);
            }
            var lexer = new Lexer(script);
            lexer.SetContext(_context);
            Execute(() =>
            {
                tokens = lexer.Tokenize();
            },
            () => string.Format("Last token: {0}, Line : {1}, Pos : {2} ", lexer.LastToken.Text, lexer.State.Line, lexer.State.LineCharPosition));
            return tokens;
        }


        /// <summary>
        /// Convert the script to a series of tokens.
        /// </summary>
        /// <param name="script">The script content or file name</param>
        /// <param name="isFile">Whether or not the script supplied is a filename or actual script content</param>
        /// <returns></returns>
        public List<Expr> ToStatements(string script, bool isFile)
        {
            List<Expr> statements = null;
            Execute(() =>
            {
                statements = _parser.Parse(script);
            });
            return statements;            
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        /// <param name="scriptFile">The source script file</param>
        /// <param name="toFile">The file to write the token info to.</param>
        public void PrintTokens(string scriptFile, string toFile)
        {
            var tokens = ToTokens(scriptFile, true);
            using (var writer = new StreamWriter(toFile))
            {
                foreach (TokenData tokendata in tokens)
                {
                    writer.WriteLine(tokendata.ToString());
                }
                writer.Flush();
            }
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        /// <param name="scriptFile">The source script file</param>
        /// <param name="toFile">The file to write the statement info to.</param>
        public void PrintStatements(string scriptFile, string toFile)
        {
            var statements = ToStatements(scriptFile, true);
            using (var writer = new StreamWriter(toFile))
            {
                foreach (Expr stmt in statements)
                {
                    writer.Write(stmt.AsString());
                }
                writer.Flush();
            }
        }


        /// <summary>
        /// Prints the run result to the file path specified.
        /// </summary>
        /// <param name="toFile"></param>
        public void PrintRunResult(string toFile)
        {
            using (var writer = new StreamWriter(toFile))
            {
                writer.Write(_runResult.ToString());
                writer.Flush();
            }
        }


        /// <summary>
        /// Prints all the meta plugins loaded.
        /// </summary>
        public void PrintPlugins()
        {
            var printer = new Printer();
            printer.WriteHeader("Meta plugins ");
            printer.WriteKeyValue(true, "Folder: ", false, this.Settings.PluginsFolder);
            printer.WriteLines(2);

            this.Context.PluginsMeta.EachPlugin( plugin =>
            {
                printer.WriteKeyValue(true, "Name: " , true, plugin.Name);
                printer.WriteKeyValue(true, "Desc: " , false, plugin.Desc);
                printer.WriteKeyValue(true, "Docs: " , false, plugin.Doc);
                printer.WriteKeyValue(true, "Type: ",  false, plugin.PluginType);
                printer.WriteKeyValue(true, "Examples: ", false, string.Empty);
                for(var ndx = 0; ndx < plugin.Examples.Length; ndx++)
                {
                    var count = (ndx + 1).ToString(CultureInfo.InvariantCulture);
                    printer.WriteLine(count + ". " + plugin.Examples[ndx]);
                }
                printer.WriteLines(3);
            });
        }


        #region Private methods
        private string ReadFile(string scriptPath)
        {
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException(scriptPath);

            var script = File.ReadAllText(scriptPath);
            return script;
        }


        private void Execute(Action action, Func<string> exceptionMessageFetcher = null)
        {
            DateTime start = DateTime.Now;
            bool success = true;
            string message = string.Empty;  
            Exception scriptError = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                success = false;
                if (ex is LangException)
                {
                    var lex = ex as LangException;
                    const string langerror = "{0} : {1} at line : {2}, position: {3}";
                    message = string.Format(langerror, lex.Error.ErrorType, lex.Message, lex.Error.Line, lex.Error.Column);
                }
                else message = ex.Message;

                scriptError = ex;
                if (exceptionMessageFetcher != null)
                    message += exceptionMessageFetcher();
            }
            DateTime end = DateTime.Now;
            _runResult = new RunResult(start, end, success, message);
            _runResult.Ex = scriptError;
        }


        private void InitPlugins()
        {
            var totalPlugins = _context.Plugins.Total();
            var tokenIt = _parser.TokenIt;
            var lexer = _parser.Lexer;

            if (!_pluginsInitialized)
            {
                // 2. Register default methods if not present.
                Tokens.Default();
                ErrorCodes.Init();
                LTypesLookup.Init();
                _context.Methods.RegisterIfNotPresent(LTypes.Array, new LJSArrayMethods());
                _context.Methods.RegisterIfNotPresent(LTypes.Date, new LJSDateMethods());
                _context.Methods.RegisterIfNotPresent(LTypes.String, new LJSStringMethods());
                _context.Methods.RegisterIfNotPresent(LTypes.Time, new LJSTimeMethods());
                _context.Methods.RegisterIfNotPresent(LTypes.Map, new LJSMapMethods());
            }

            if (!_pluginsInitialized || totalPlugins > _lastInitializationPluginCount)
            {
                // 3. Initialize the plugins.
                var expParser = new ExprParser();
                expParser._parser = _parser;
                _context.Plugins.RegisterAllSystem();
                _context.Plugins.ForEach<IExprPlugin>(plugin =>
                                                          {
                                                              plugin.Init(_parser, tokenIt);
                                                              plugin.Ctx = _context;
                                                              plugin.ExpParser = expParser;
                                                          });

                _context.Plugins.ForEach<ITokenPlugin>(plugin => { plugin.Init(_parser, tokenIt); });
                _context.Plugins.ForEach<ILexPlugin>(plugin => { plugin.Init(lexer); });
                _context.Plugins.ExecuteSetupPlugins(_context);

                _lastInitializationPluginCount = _context.Plugins.Total();
            }
            _pluginsInitialized = true;
        }


        private void InitSystemFunctions()
        {
            // Print and log functions.
            _parser.Context.ExternalFunctions.Register("print",  (objname, method, exp) => LogHelper.Print(_settings, exp, false));
            _parser.Context.ExternalFunctions.Register("println", (objname, method, exp) => LogHelper.Print(_settings, exp, true));
            _parser.Context.ExternalFunctions.Register("log.*", (objname, method, exp) => LogHelper.Log(_settings, exp));
            _parser.Context.ExternalFunctions.Register("metacompiler.*", (objname, method, exp) => RunCompilerMethod(objname, method, _settings, exp));
        }



        private object RunCompilerMethod(string objectName, string method, LangSettings settings, FunctionCallExpr ex)
        {
            var metaCompiler = new MetaCompiler();
            metaCompiler.Ctx = ex.Ctx;

            if(method == "ToConstDate")
            {
            
            }
            else if(method == "ToConstTime")
            {
                
            }
            else if(method == "ToConstDateTimeToken")
            {
                var dateToken = ex.ParamList[0] as TokenData;
                var timeToken = ex.ParamList[1] as TokenData;
                return metaCompiler.ToConstDateTimeToken(dateToken, timeToken);
            }
            else if (method == "ToConstDay")
            {
                var token = ex.ParamList[0] as TokenData;
                var day = Convert.ToInt32(ex.ParamList[1]);
                return metaCompiler.ToConstDay(day, token);
            }
            return LObjects.Null;
        }
        #endregion
    }



    class Printer
    {
        /// <summary>
        /// Writes out a header text
        /// </summary>
        /// <param name="text"></param>
        public void WriteHeader(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text.ToUpper());
        }


        /// <summary>
        /// Writest the text supplied on 1 line.
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }


        /// <summary>
        /// Writes out a key/value line.
        /// </summary>
        /// <param name="highlightKey"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void WriteKeyValue(bool highlightKey, string key, bool highlightVal, string val)
        {
            if (highlightKey)
                Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(key);
            Console.ResetColor();
            if(highlightVal)
                Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(val);
            Console.ResetColor();
        }


        /// <summary>
        /// Writes out lines
        /// </summary>
        /// <param name="count"></param>
        public void WriteLines(int count)
        {
            for (int ndx = 0; ndx < count; ndx++)
            {
                Console.WriteLine();
            }
        }
    }
    
}
