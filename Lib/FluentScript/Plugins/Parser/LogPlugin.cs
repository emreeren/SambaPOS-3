using System;
using System.Collections.Generic;
using System.IO;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>
    
    // CONFIGURATION    
    // 1. Set level to "error" and log to console
    log.configure( 'error', 'console' )
    
    // 2. Fluen mode, set level = "error" and log to console.
    log configure 'error', 'console'
    
    // 3. File logging: set level to "warn" and log to file
    log configure 'warn', 'c:/temp/myapp.log'
    
    // 4. File logging: set level to "warn" and log to file with a name format.
    // log file name will be converted to "myapp-2012-07-15-time-09-45-12.log"
    log configure 'warn', 'c:/temp/myapp.log', '${yyyy-MM-dd}-time-${HH-mm-ss}'
    
    // 5. Get the current log level : this is only a getter property
    print( log.level )
    
    
    // LOGGING   
    // 1. Using OO ( object oriented ) syntax e.g. "log.<method>"
    log.error( 'an error occurred' )
    
    // 2. Using OO syntax with formatting
    log.warn( 'could not load {0}', 'blogs' )
    
    // 3. Using OO syntax in fluent mode
    log info 'finished updating data'
    
    // 4. functional mode - without prefixing with "log."
    error 'unable to initialize'
    warn 'could not send notification'
    info 'finished updating data'
    
    // NOTES: Log-level
    // 1. fatal
    // 2. error
    // 3. warn
    // 4. info
    // 5. debug
    // 6. put   ( this will always output the log message )    
    </doc:example>
    ***************************************************************************/
    /// <summary>
    /// Constants used for the log plugin
    /// </summary>
    public class LogPluginConstants
    {
        private static Dictionary<string, int> _methodMap;

        // Error levels
        public const int Put = 6; 
        public const int Fatal = 5;
        public const int Error = 4;
        public const int Warn  = 3;
        public const int Info  = 2;
        public const int Debug = 1;

        // For output modes
        public const int Console = 1;
        public const int File = 2;
        public const int Callback = 3;


        static LogPluginConstants()
        {
            _methodMap = new Dictionary<string, int>();
            _methodMap["critical"]  = LogPluginConstants.Fatal;
            _methodMap["error"]     = LogPluginConstants.Error;
            _methodMap["warn"]      = LogPluginConstants.Warn;
            _methodMap["info"]      = LogPluginConstants.Info;
            _methodMap["debug"]     = LogPluginConstants.Debug;
            _methodMap["put"]       = LogPluginConstants.Put;
        }


        internal static bool ContainsKey(string key)
        {
            return _methodMap.ContainsKey(key);
        }


        internal static int LevelFor(string key)
        {
            return _methodMap[key];
        }
    }



    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class LogPlugin : ExprPlugin, IDisposable
    {
        /// <summary>
        ///  This is used if the logging is set an external c# method.
        ///  So it's not logging to console or file, this is useful for unit-testing.
        /// </summary>
        private Action<int, string, LError> _callback;


        /// <summary>
        /// Initialize
        /// </summary>
        public LogPlugin() : this(null)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public LogPlugin(Action<int, string, LError> callback)
        {
            _callback = callback;
            this.StartTokens = new string[]{ "log", "put", "fatal", "error", "warn", "info", "debug" };
            this.IsAutoMatched = true;
            this.IsStatement = true;
            this.IsEndOfStatementRequired = true;
            this.Precedence = 100;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "log '.'? ( 'fatal' | 'error' | 'warn' | 'info' | 'debug' | 'put' ) '(' <expr> ( ',' <expr> ) ')'";
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
                    "log.fatal ( 'testing' );", 
                    "log.error ( 'testing' );",
                    "log.error ( 'testing', err );",
                    "log.warn  ( 'testing', err );",
                    "log.info  ( 'testing', err );",
                    "log.debug ( 'testing', err );", 
                    "log.put   ( 'testing', err );", 
                };
            }
        }


        /// <summary>
        /// Checks whether or not this plugin can handle the current token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            if (!(current.Kind == TokenKind.Ident))
                return false;

            if (current.Text == "log" || LogPluginConstants.ContainsKey(current.Text))
                return true;

            return false;
        }


        /// <summary>
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            LogExpr logexpr = null;
            
            // CASE 1: logging using OO mode e.g. log.warn ( 'some message' )
            if (_tokenIt.NextToken.Token.Text == "log")
            {
                _tokenIt.Advance();

                // Move past the "." and check the log level in the next if stmt.
                if (_tokenIt.NextToken.Token == Tokens.Dot)
                    _tokenIt.Advance();
            }
            // CASE 2: logging using function mode e.g warn( 'some message' )
            if (LogPluginConstants.ContainsKey(_tokenIt.NextToken.Token.Text))
            {
                logexpr = new LogExpr() { Mode = "log", LogLevel = _tokenIt.NextToken.Token.Text };
            }
            // CASE 3: Asking for log level
            else if (_tokenIt.NextToken.Token.Text == "level")
            {
                logexpr = new LogExpr() { Mode = "level" };
            }
            // CASE 4: configuring the logger via parameters.
            else if (_tokenIt.NextToken.Token.Text == "configure")
            {
                logexpr = new LogExpr() { Mode = "configure" };
            }
            else
                throw _tokenIt.BuildSyntaxUnexpectedTokenException();

            logexpr.Callback = _callback;
                
            // Move to parameters.
            _tokenIt.Advance();
            bool expectParenthesis = _tokenIt.NextToken.Token == Tokens.LeftParenthesis;
            _parser.ParseParameters(logexpr, expectParenthesis, true, true);
            return logexpr;
        }


        /// <summary>
        /// Shutsdown the log plugin by closing any open file resources.
        /// </summary>
        public void Dispose()
        {
            var settings = _parser.Context.Plugins.GetSettings<LogExpr.LogSettings>("comlib.log");
            LogExpr.Dispose(settings);
        }        
    }



    /// <summary>
    /// New instance creation.
    /// </summary>
    public class LogExpr : Expr, IParameterExpression
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LogExpr()
        {
            ParamList = new List<object>();
            ParamListExpressions = new List<Expr>();
        }


        /// <summary>
        /// The log mode, currently this expression supports 
        /// 1.  level assignment  : log level = error 
        /// 2a. output assignment : log to console 
        /// 2b. output assignment : log to c:\temp.txt, 'myapp-${yyyy-mm-dd}-time-${hh-mm-ss}' 
        /// 3.  log message       : log error 'an error occurred' 
        /// </summary>
        public string Mode { get; set; }


        /// <summary>
        /// The log level e.g. Error, Warn. etc.
        /// </summary>
        public string LogLevel { get; set; }


        /// <summary>
        /// Callback for logging.
        /// </summary>
        public Action<int, string, LError> Callback;


        /// <summary>
        /// List of expressions.
        /// </summary>
        public List<Expr> ParamListExpressions { get; set; }


        /// <summary>
        /// List of arguments.
        /// </summary>
        public List<object> ParamList { get; set; }


        /// <summary>
        /// Creates new instance of the type.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            var settings = Ctx.Plugins.GetSettings<LogSettings>("comlib.log");
            if (settings == null)
            {
                settings = new LogSettings();
                Ctx.Plugins.SetSettings("comlib.log", settings);
            }

            // 1. Resolve the parameters.
            ParamHelper.ResolveNonNamedParameters(ParamListExpressions, ParamList, visitor);
            
            if (Mode == "log")
            {
                Log(settings);
                return LObjects.EmptyString;
            }
            else if (Mode == "level")
            {
                return new LString(settings.LogLevelName);
            }
            else if (Mode == "level_check")
            {
                var level = this.LogLevel;
                if (level == "debug") return settings.LogLevelValue == LogPluginConstants.Debug;
                if (level == "info") return settings.LogLevelValue == LogPluginConstants.Info;
                if (level == "warn") return settings.LogLevelValue == LogPluginConstants.Warn;
                if (level == "error") return settings.LogLevelValue == LogPluginConstants.Error;
                if (level == "fatal") return settings.LogLevelValue == LogPluginConstants.Fatal;
                return false;
            }
            else if (Mode == "configure")
            {
                Configure(settings);
                return LObjects.EmptyString;
            }
            return LObjects.EmptyString;
        }


        private void Configure(LogSettings settings)
        {
            ExceptionHelper.NotNullType(this, this.ParamList[0], "log level not supplied", LTypes.String);
            ExceptionHelper.NotNullType(this, this.ParamList[1], "console or log not supplied", LTypes.String);

            // Param 1: Error level
            settings.LogLevelName = ((LString)this.ParamList[0]).Value;
            settings.LogLevelValue = LogPluginConstants.LevelFor(settings.LogLevelName);

            // Param 2: Console or file?
            var output = ((LString)this.ParamList[1]).Value;
            if (string.Compare(output, "console", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                settings.OutputMode = LogPluginConstants.Console;
            }
            else if (string.Compare(output, "callback", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                settings.OutputMode = LogPluginConstants.Callback;
                settings.Callback = Callback;
            }
            else
            {
                // Close any existing log
                Dispose(settings);
                SetupFileLog(settings);
            }
        }


        private void Log(LogSettings settings)
        {
            // Only log if the log level is appropriate
            var level = LogPluginConstants.LevelFor(LogLevel);

            // Validate: log.debug but level is Warn
            if (level < settings.LogLevelValue)
                return;

            // Good to log.
            string message = LogHelper.BuildMessage(ParamList);
            if (settings.OutputMode == LogPluginConstants.Console)
                Console.WriteLine(LogLevel + " : " + message);
            else if (settings.OutputMode == LogPluginConstants.Callback)
                settings.Callback(level, message, null);
            else if (settings.OutputMode == LogPluginConstants.File && settings.Logger != null)
                settings.Logger.WriteLine(LogLevel + " : " + message);            
        }


        private void SetupFileLog(LogSettings settings)
        {
            settings.OutputMode = LogPluginConstants.File;

            // 1st param
            var filename = ((LString)ParamList[1]).Value;
            var file = new FileInfo(filename);
            var name = file.Name.Replace(file.Extension, string.Empty);

            if (ParamList.Count > 2)
            {
                string format = Convert.ToString(ParamList[2]);
                format = format.Replace("${yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                format = format.Replace("${HH-mm-ss}", DateTime.Now.ToString("HH-mm-ss"));
                format = format.Replace("${yyyy}", DateTime.Now.ToString("yyyy"));
                format = format.Replace("${yy}", DateTime.Now.ToString("yy"));
                format = format.Replace("${MM}", DateTime.Now.ToString("MM"));
                format = format.Replace("${dd}", DateTime.Now.ToString("dd"));
                format = format.Replace("${HH}", DateTime.Now.ToString("HH"));
                format = format.Replace("${mm}", DateTime.Now.ToString("mm"));
                format = format.Replace("${ss}", DateTime.Now.ToString("ss"));
                name = name + "-" + format + file.Extension;
            }
            settings.FileName = file.Directory.FullName + Path.DirectorySeparatorChar + name;

            try
            {
                settings.Logger = new StreamWriter(settings.FileName);
                settings.Logger.WriteLine("Starting log at : " + DateTime.Now.ToString("yyyy-MM-dd at HH-mm-ss"));
                settings.Logger.Flush();
            }
            catch (Exception)
            {
                throw BuildRunTimeException("Unable to log to file : " + settings.FileName);
            }
        }


        /// <summary>
        /// Shutsdown the log plugin by closing any open file resources.
        /// </summary>
        internal static void Dispose(LogExpr.LogSettings settings)
        {
            try
            {
                if (settings != null && settings.Logger != null)
                {
                    settings.Logger.Flush();
                    settings.Logger.Dispose();
                }
            }
            catch (Exception)
            {
            }
        } 


        internal class LogSettings
        {
            /// <summary>
            /// The log level
            /// </summary>
            public string LogLevelName = "INFO";


            /// The log level value
            public int LogLevelValue = LogPluginConstants.Debug;


            /// <summary>
            /// Whether or not outputting to console.
            /// </summary>
            public int OutputMode = LogPluginConstants.Console;


            /// <summary>
            /// Filename if outputting to file.
            /// </summary>
            public string FileName = string.Empty;


            /// <summary>
            /// Writer to file.
            /// </summary>
            public StreamWriter Logger;


            /// <summary>
            /// Used to call an external c# method
            /// </summary>
            public Action<int, string, LError> Callback;
        }
    } 
}
