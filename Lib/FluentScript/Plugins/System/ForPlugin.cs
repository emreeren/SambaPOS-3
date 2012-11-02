using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /* *************************************************************************
    <doc:example>	
    // Return plugin provides return values
    
    return false;
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class ForLoopPlugin : ExprBlockPlugin
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public ForLoopPlugin()
        {
            this.ConfigureAsSystemStatement(true, false, "for");
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "for '(' ( ( <id> in <expression> ) | ( <id> '=' <expression> ';' <id> <op> <expression> ';' <id> <op> <expression>? ) ) ')' <statementblock>"; }
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
                    "for ( num in numbers ) { ... }",
                    "for ( num = 1; num < 10; num++ ) { ... }",
                    "for ( num = 1; num < 10; num += 2 ) { ... }"
                };
            }
        }


        /// <summary>
        /// Parses either the for or for x in statements.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            _tokenIt.ExpectMany(Tokens.For, Tokens.LeftParenthesis);
            var ahead = _tokenIt.Peek(1);
            if (ahead.Token == Tokens.In) return ParseForIn();

            return ParseForLoop();
        }


        private Expr ParseForLoop()
        {
            var stmt = new ForExpr();
            var statements = new List<Expr>();

            // While ( condition expression )
            // Parse while condition
            var start = _parser.ParseStatement();            
            var condition = _parser.ParseExpression(Terminators.ExpSemicolonEnd);
            _tokenIt.Advance();
            string name = _tokenIt.ExpectId();
            var increment = _parser.ParseUnary(name, false);
            _tokenIt.Expect(Tokens.RightParenthesis);
            stmt.Init(start, condition, increment);
            ParseBlock(stmt);
            return stmt;
        }


        /// <summary>
        /// return value;
        /// </summary>
        /// <returns></returns>
        private Expr ParseForIn()
        {
            var varname = _tokenIt.ExpectId();
            _tokenIt.Expect(Tokens.In);
            var sourcename = _tokenIt.ExpectId();
            _tokenIt.Expect(Tokens.RightParenthesis);
            var stmt = new ForEachExpr(varname, sourcename);
            ParseBlock(stmt);
            return stmt;
        }
    }



    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ForExpr : WhileExpr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public ForExpr()
            : this(null, null, null)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="start">start expression</param>
        /// <param name="condition">condition for loop</param>
        /// <param name="inc">increment expression</param>
        public ForExpr(Expr start, Expr condition, Expr inc)
            : base(condition)
        {
            InitBoundary(true, "}");
            Init(start, condition, inc);
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="start">start expression</param>
        /// <param name="condition">condition for loop</param>
        /// <param name="inc">increment expression</param>
        public void Init(Expr start, Expr condition, Expr inc)
        {
            Start = start;
            Increment = inc;
            Condition = condition;
        }


        /// <summary>
        /// Start statement.
        /// </summary>
        public Expr Start;


        /// <summary>
        /// Increment statement.
        /// </summary>
        public Expr Increment;



        /// <summary>
        /// Execute each expression.
        /// </summary>
        /// <returns></returns>
        public override object Evaluate()
        {
            Start.Evaluate();
            _continueRunning = true;
            _breakLoop = false;
            _continueLoop = false;
            _continueRunning = Condition.EvaluateAs<bool>();

            while (_continueRunning)
            {
                if (_statements != null && _statements.Count > 0)
                {
                    foreach (var stmt in _statements)
                    {
                        stmt.Evaluate();

                        Ctx.Limits.CheckLoop(this);

                        // If Break statment executed.
                        if (_breakLoop)
                        {
                            _continueRunning = false;
                            break;
                        }
                        // Continue statement.
                        else if (_continueLoop)
                            break;
                    }
                }
                else break;

                // Break loop here.
                if (_continueRunning == false)
                    break;

                Increment.Evaluate();
                _continueRunning = Condition.EvaluateAs<bool>();
            }
            return LNull.Instance;
        }
    } 



    /// <summary>
    /// For loop Expression data
    /// </summary>
    public class ForEachExpr : WhileExpr
    {
        private string _varName;
        private string _sourceName;


        /// <summary>
        /// Initialize using the variable names.
        /// </summary>
        /// <param name="varname">Name of the variable in the loop</param>
        /// <param name="sourceName">Name of the variable containing the items to loop through.</param>
        public ForEachExpr(string varname, string sourceName)
            : base(null)
        {
            _varName = varname;
            _sourceName = sourceName;
        }


        /// <summary>
        /// Execute each expression.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            _continueRunning = true;
            _breakLoop = false;
            _continueLoop = false;

            // for(user in users)
            // Push scope for var name 
            object source = Ctx.Memory.Get<object>(_sourceName);

            IEnumerator enumerator = null;
            if (source is LArray) enumerator = ((LArray)source).Raw.GetEnumerator();
            else if (source is IList) enumerator = ((IList)source).GetEnumerator();
            else if (source is Array) enumerator = ((Array)source).GetEnumerator();
            else if (source is LMap) enumerator = ((LMap)source).Raw.GetEnumerator();

            _continueRunning = enumerator.MoveNext();

            while (_continueRunning)
            {
                // Set the next value of "x" in for(x in y).
                Ctx.Memory.SetValue(_varName, enumerator.Current);

                if (_statements != null && _statements.Count > 0)
                {
                    foreach (var stmt in _statements)
                    {
                        stmt.Evaluate();

                        Ctx.Limits.CheckLoop(this);
                         
                        // If Break statment executed.
                        if (_breakLoop)
                        {
                            _continueRunning = false;
                            break;
                        }
                        // Continue statement.
                        else if (_continueLoop)
                            break;
                    }
                }
                else break;

                // Break loop here.
                if (_continueRunning == false)
                    break;

                // Increment.
                _continueRunning = enumerator.MoveNext();
            }
            return LNull.Instance;
        }
    }    
}
