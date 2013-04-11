using System;
using System.Collections.Generic;
using System.Linq;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Aggregate plugin allows sum, min, max, avg, count aggregate functions to 
    // be applied to lists of objects.
    
    var numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9];
    var result = 0;
    
    // Example 1: Using format sum of <expression>
    result = sum of numbers;
    result = avg of numbers;
    result = min of numbers;
    result = max of numbers;
    result = count of numbers;
    
    // Example 2: Using format sum(<expression>)
    result = sum( numbers );
    result = avg( numbers );
    result = min( numbers );
    result = max( numbers );
    result = count( numbers );    
    </doc:example>
    ***************************************************************************/
    // <fs:plugin-autogenerate>
    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class AggregatePlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public AggregatePlugin()
        {
            this.IsAutoMatched = true;
            this.StartTokens = new string[] 
            { 
                "avg", "min", "max", "sum", "count", "number", 
                "Avg", "Min", "Max", "Sum", "Count", "Number"
            };
        }



        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "( avg | min | max | sum | count | number ) ( ( '(' <expression> ')' ) | ( of <expression> ) )"; }
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
                    "min( numbers )",
                    "Min( numbers )",
                    "min of numbers",
                    "Min of numbers"
                };
            }
        }
        // </fs:plugin-autogenerate>


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var next = _tokenIt.Peek().Token;
            if (next == Tokens.LeftParenthesis || string.Compare(next.Text, "of", StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;
            return false;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // avg min max sum count
            string aggregate = _tokenIt.NextToken.Token.Text.ToLower();

            var next = _tokenIt.Peek().Token;
            Expr exp = null;

            // 1. sum( <expression> )
            if (next == Tokens.LeftParenthesis)
            {
                _tokenIt.Advance(2);
                exp = _parser.ParseExpression(Terminators.ExpParenthesisEnd, passNewLine: false);
                _tokenIt.Expect(Tokens.RightParenthesis);
            }
            // 2. sum of <expression>
            else if (string.Compare(next.Text, "of", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                _tokenIt.Advance(2);
                exp = _parser.ParseExpression(null, false, true, passNewLine: false);
            }
            
            var aggExp = new AggregateExpr(aggregate, exp);
            return aggExp;
        }
    }



    /// <summary>
    /// Expression to represent a Linq like query.
    /// </summary>
    public class AggregateExpr : Expr
    {        
        private string _aggregateType;
        private Expr _source;


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="aggregateType">sum avg min max count total</param>
        /// <param name="source"></param>
        public AggregateExpr(string aggregateType, Expr source)
        {
            this.Nodetype = "FSExtAggregate";
            this.InitBoundary(true, ")");
            this._aggregateType = aggregateType;
            this._source = source;
        }


        /// <summary>
        /// Evaluate the aggregate expression.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            var dataSource = _source.Evaluate(visitor) as LObject;
            ExceptionHelper.NotNull(this, dataSource, "aggregation(min/max)");
            
            List<object> items = null;

            // Check 1: Could have supplied a single number e.g. sum(2) so just return 2
            if (dataSource.Type == LTypes.Number)
                return dataSource.Clone();

            // Check 2: Expect array
            if (dataSource.Type != LTypes.Array)
                throw new NotSupportedException(_aggregateType + " not supported for list type of " + dataSource.GetType());

            items = dataSource.GetValue() as List<object>;
            var val = 0.0;
            if (_aggregateType == "sum")
                val = items.Sum(item => GetValue(item));

            else if (_aggregateType == "avg")
                val = items.Average(item => GetValue(item));

            else if (_aggregateType == "min")
                val = items.Min(item => GetValue(item));

            else if (_aggregateType == "max")
                val = items.Max(item => GetValue(item));

            else if (_aggregateType == "count" || _aggregateType == "number")
                val = items.Count;

            return new LNumber(val);
        }


        private double GetValue(object item)
        {
            // Check 1: Null
            if (item == LObjects.Null) return 0;
            var lobj = (LObject) item;

            // Check 2: Number ? ok
            if (lobj.Type == LTypes.Number) return ((LNumber) lobj).Value;

            return 0;
        }
    }
}