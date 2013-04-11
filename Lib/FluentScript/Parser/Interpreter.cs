using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.Integration;
using Fluentscript.Lib.Phases;
using Fluentscript.Lib.Runtime;
using Fluentscript.Lib.Runtime.Bindings;
using Fluentscript.Lib.Types;
using Fluentscript.Lib.Types.Javascript;
using Fluentscript.Lib._Core;
using Fluentscript.Lib._Core.Meta.Types;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
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
        /// Registers all the plugins.
        /// </summary>
        public void RegisterAllPluginsForDevice()
        {
            this.Context.Plugins.RegisterAllSystem();
            this.RegisterMetaPlugins();
            this.Context.Plugins.RegisterAllCustomForDevice();
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
                    var script = ReadFile(file);
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
        /// Append the script to the existing code and executes only the new code.
        /// </summary>
        /// <param name="scriptPath"></param>
        public void AppendExecuteFile(string scriptPath)
        {
            var script = ReadFile(scriptPath);
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
            if (clearExistingCode || _phaseCtx == null)
            {
                _phaseCtx = new PhaseContext();
                _phaseCtx.Ctx = _context;
            }
            if (resetScript)
            {
                _phaseCtx.ScriptText = script;
            }
            var phasesList = phases.ToList();
            var result = phaseExecutor.Execute(script, _phaseCtx, _context, phasesList);
            this._runResult = result.Result;
        }


        /// <summary>
        /// Loads the arguments supplied into the runtime.
        /// </summary>
        /// <param name="args">The metadata of the arguments.</param>
        /// <param name="argValues">The argument values as strings</param>
        public RunResult LoadArguments(List<ArgAttribute> args, List<string> argValues)
        {
            var errors = new List<ScriptError>();
            var start = DateTime.Now;
            for(var ndx = 0; ndx < args.Count; ndx++)
            {
                var arg = args[ndx];
                var argVal = argValues[ndx];
                try
                {
                    var langType = LangTypeHelper.ConvertToLangTypeFromLangTypeName(arg.Type);
                    var langValueText = argVal;
                    if (string.IsNullOrEmpty(argVal) && !arg.Required && arg.DefaultValue != null)
                        langValueText = Convert.ToString(arg.DefaultValue);

                    var langValue = LangTypeHelper.ConvertToLangValue(langType, langValueText);
                    this.Context.Memory.SetValue(arg.Name, langValue, false);
                    this.Context.Symbols.DefineVariable(arg.Name, langType);
                }
                catch (Exception)
                {
                    var error = new ScriptError();
                    error.Message = "Unable to create variable : " + arg.Name + " with value : " + argVal;
                    errors.Add(error);
                    throw;
                }
            }
            var end = DateTime.Now;
            var result = new RunResult(start, end, errors.Count == 0, errors);
            return result;
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
                script = ReadFile(script);
            }

            Lexer lexer = null;
            if (_parser == null)
            {
                lexer = new Lexer(script);
                lexer.SetContext(_context);
            }
            else
            {
                lexer = _parser.Lexer;
                lexer.Init(script);
            }

            //lexer.DiagnosticData.Reset();

            Execute(() =>
            {
                tokens = lexer.Tokenize();
            },
            () => string.Format("Last token: {0}, Line : {1}, Pos : {2} ", lexer.LastToken.Text, lexer.State.Line, lexer.State.LineCharPosition));


            //Console.WriteLine("total : " + lexer.DiagnosticData.TotalTokens);
            //Console.WriteLine("total white space: " + lexer.DiagnosticData.TotalWhiteSpaceTokens);
            //Console.WriteLine("total new line   : " + lexer.DiagnosticData.TotalNewLineTokens);
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


        public void InitPlugins()
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
                _context.Methods.RegisterIfNotPresent(LTypes.Table, new LJSTableMethods());
            }

            if (!_pluginsInitialized || totalPlugins > _lastInitializationPluginCount)
            {
                // 3. Initialize the plugins.
                var expParser = new ExprParser();
                expParser._parser = _parser;
                _context.PluginsMeta.Parser = expParser;
                _context.Plugins.RegisterAllSystem();
                _context.Plugins.ForEach<IExprPlugin>(plugin =>
                                                          {
                                                              plugin.Init(_parser, tokenIt);
                                                              plugin.Ctx = _context;
                                                              plugin.ExpParser = expParser;
                                                          });

                _context.Plugins.ForEach<ITokenPlugin>(plugin => { plugin.Init(_parser, tokenIt); });
                _context.Plugins.ForEach<ILexPlugin>(plugin => { plugin.Init(lexer); plugin.Ctx = _context; });
                _context.Plugins.ExecuteSetupPlugins(_context);

                PreprocessHelper.Ctx = _context;
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


        /// <summary>
        /// Runs the compiler hook/function call on the compiler languge binding class.
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="method"></param>
        /// <param name="settings"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        private object RunCompilerMethod(string objectName, string method, LangSettings settings, FunctionCallExpr expr)
        {
            var binding = new MetaCompiler();
            binding.Ctx = expr.Ctx;
            binding.ExecuteFunction(method, new object[] {expr});
            return LObjects.Null;
        }
        #endregion
        
    }
}
