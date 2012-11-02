using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

using ComLib.Lang.Helpers;

namespace ComLib.Lang
{
    /*
    ConstCapsPlugin 		- $Identifier
    DatePlugin 				- $DateToken
    DateNumberPlugin 		- $NumberToken
    EmailPlugin 			- $IdToken
    FluentFuncPlugin 		- $IdToken
    FluentMemberPlugin 		- $IdToken
    FuncWildCardPlugin 		- $IdToken
    LinqPlugin 				- $IdToken
    MoneyPlugin 			- $
    PercentPlugin 			- $Suffix
    SuffixPlugin 			- $Suffix
    TimePlugin 				- $NumberToken
    UnitsPlugin 			- $Suffix
    UriPlugin 				- $IdToken
    VariablePathPlugin 		- $IdToken
    WordsInterpretPlugin 	- $IdToken

    $Kind:Identifier
    $Kind:Number
    $Kind:Date
    */
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
        /// Register the callback for custom functions
        /// </summary>
        /// <param name="funcCallPattern">Pattern for the function e.g. "CreateUser", or "Blog.*"</param>
        /// <param name="callback">The callback to call</param>
        public void SetFunctionCallback(string funcCallPattern, Func<FunctionCallExpr, object> callback)
        {
            _parser.Context.ExternalFunctions.Register(funcCallPattern, callback);
        }


        /// <summary>
        /// Parses the script but does not execute it.
        /// </summary>
        /// <param name="scriptPath">Path to the script</param>
        public void ParseFile(string scriptPath)
        {
            Execute(() =>
            {
                var script = ReadFile(scriptPath);
                Parse(script);
            });
        }


        /// <summary>
        /// Parses the script but does not execute it.
        /// </summary>
        /// <param name="script"></param>
        public void Parse(string script)
        {
            Execute(() =>
            {
                _context.Limits.CheckScriptLength(script);
                _parser.Parse(script, _memory);
            });
        }


        /// <summary>
        /// Executes the file.
        /// </summary>
        /// <param name="scriptPath">Path to the script</param>
        public void ExecuteFile(string scriptPath)
        {
            Execute(() =>
            {
                var script = ReadFile(scriptPath);
                Execute(script);
            });
        }


        /// <summary>
        /// Executes the script
        /// </summary>
        /// <param name="script">Script text</param>
        public void Execute(string script)
        {
            Execute(() =>
            {
                _context.Limits.CheckScriptLength(script);
                _parser.Parse(script, _memory);
                _parser.Execute();
            });
        }


        /// <summary>
        /// Call a fluent script function from c#.
        /// </summary>
        /// <param name="functionName">The name of the function to call</param>
        /// <param name="convertApplicableTypes">Whether or not to convert applicable c# types to fluentscript types, eg. ints and longs to double, List(object) to LArray and Dictionary(string, object) to LMap</param>
        /// <param name="args"></param>
        public object Call(string functionName, bool convertApplicableTypes, params object[] args)
        {
            var argsList = args.ToList<object>();
            if(convertApplicableTypes)
                FunctionHelper.ConvertToFluentScriptTypes(argsList);
            var result = _context.Functions.CallByName(functionName, null, argsList, false);
            return result;
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
            var lexer = new Lexer(_context, script);

            Execute(() =>
            {
                tokens = lexer.Tokenize();
            },
            () => string.Format("Last token: {0}, Line : {1}, Pos : {2} ", lexer.LastToken.Text, lexer.LineNumber, lexer.LineCharPos));
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
            using (StreamWriter writer = new StreamWriter(toFile))
            {
                foreach (TokenData tokendata in tokens)
                {
                    writer.WriteLine(tokendata.ToString());
                }
                writer.Flush();
            };
        }


        /// <summary>
        /// Prints tokens to file supplied, if file is not supplied, prints to console.
        /// </summary>
        /// <param name="scriptFile">The source script file</param>
        /// <param name="toFile">The file to write the statement info to.</param>
        public void PrintStatements(string scriptFile, string toFile)
        {
            var statements = ToStatements(scriptFile, true);
            using (StreamWriter writer = new StreamWriter(toFile))
            {
                foreach (Expr stmt in statements)
                {
                    writer.Write(stmt.AsString());
                }
                writer.Flush();
            };
        }


        /// <summary>
        /// Prints the run result to the file path specified.
        /// </summary>
        /// <param name="toFile"></param>
        public void PrintRunResult(string toFile)
        {
            using (StreamWriter writer = new StreamWriter(toFile))
            {
                writer.Write(_runResult.ToString());
                writer.Flush();
            };
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
                    LangException lex = ex as LangException;
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


        private void InitSystemFunctions()
        {
            // Print and log functions.
            _parser.Context.ExternalFunctions.Register("print",  (exp) => FunctionHelper.Print(_settings, exp, false));
            _parser.Context.ExternalFunctions.Register("printl", (exp) => FunctionHelper.Print(_settings, exp, true));
            _parser.Context.ExternalFunctions.Register("log.*",  (exp) => FunctionHelper.Log(_settings, exp));
        }
        #endregion
    }
}
