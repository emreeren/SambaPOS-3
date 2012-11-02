using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang;


namespace ComLib.Lang.Extensions
{

    /* *************************************************************************
    <doc:example>	
    // Sort plugin allows you to sort a list
    
    // Case 1: sort list of basic types
    var numbers = [4, 3, 1, 7, 5, 2, 6];
    sort numbers asc
    sort numbers desc
    
    // Case 2: start with source <books> and system auto creates variable <book>
    var books = [ 
				    { name: 'book 1', pages: 200, author: 'homey' },
				    { name: 'book 2', pages: 120, author: 'kdog' },
				    { name: 'book 3', pages: 140, author: 'homeslice' }
			    ];     
    sort books by book.pages asc
    sort books by book.pages desc
    sort i in books by i.pages desc
     
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class SortPlugin : ExprPlugin
    {
        private static IDictionary<Token, bool> _terminators;


        private string _variableName;
        private Expr _source;
        private Expr _filter;


        static SortPlugin()
        {
            _terminators = new Dictionary<Token, bool>();
            _terminators[Tokens.ToIdentifier("asc")] = true;
            _terminators[Tokens.ToIdentifier("desc")] = true;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public SortPlugin()
        {
            this.StartTokens = new string[] { "sort", "Sort" };
            this.IsAutoMatched = true;
            this.IsStatement = true;
            this.IsContextFree = false;
            this.IsEndOfStatementRequired = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "sort <expression> ( in <expression> )? by <expression> ( asc | desc )?";
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
                    "sort books by book.pages asc",
                    "sort books by book.pages desc",
                    "sort i in books by i.pages desc"
                };
            }
        }


        /// <summary>
        /// Sorts expression
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // sort books by book.pages asc
            // sort books by book.pages desc
            // sort i in books by i.pages desc         
            _tokenIt.Advance();
            
            // 1. Get the source to filter
            _source = _parser.ParseExpression(null, false, true, false);
                        
            // 2. if specified "in" then explicit naming of variable in list.
            if (string.Compare(_tokenIt.NextToken.Token.Text, "in", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                _variableName = ((VariableExpr)_source).Name;
                _tokenIt.Advance();

                // 3. Get the source to sort
                _source = _parser.ParseExpression(null, false, true, false);
            }
            else
            {
                if (_source is VariableExpr)
                {
                    _variableName = ((VariableExpr)_source).Name;
                }
                else if(_source is MemberAccessExpr)
                {
                    _variableName = ((MemberAccessExpr)_source).MemberName;
                    _variableName = _variableName.Substring(0, _variableName.Length - 1);
                }
                else if (_source is DataTypeExpr)
                {
                    _variableName = "temps";
                }
                _variableName = _variableName.Substring(0, _variableName.Length - 1);
            }

            bool isAsc;
            string ascDesc;

            // Asc or desc immediately.
            // Filter not specified if basic types in source list.            
            ascDesc = _tokenIt.NextToken.Token.Text;
            if (string.Compare(ascDesc, "asc", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(ascDesc, "desc", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                isAsc = string.Compare(ascDesc, "asc", StringComparison.InvariantCultureIgnoreCase) == 0;
                _tokenIt.Advance();
                return new SortExpr(_variableName, _source, _filter, isAsc);
            }

            // 4. Finally "by" <filter> asc | desc
            _tokenIt.ExpectIdText("by");
            _filter = _parser.ParseExpression(null, false, true, false);

            // 5. asc | desc
            ascDesc = _tokenIt.NextToken.Token.Text;
            isAsc = string.Compare(ascDesc, "asc", StringComparison.InvariantCultureIgnoreCase) == 0;
            
            // Finally move past the plugin
            _tokenIt.Advance();
            return new SortExpr(_variableName, _source, _filter, isAsc);
        }
    }



    /// <summary>
    /// Expression to represent a Linq like query.
    /// </summary>
    public class SortExpr : IndexableExpr
    {
        private Expr _source = null;
        private string _varName = null;
        private Expr _filter = null;
        private bool _isAsc;


        /// <summary>
        /// Initialize using values.
        /// </summary>
        /// <param name="varName">The name of the variable representing each item in the source</param>
        /// <param name="source">The data source being queried</param>
        /// <param name="filter">The filter to apply on the datasource</param>
        /// <param name="isAsc">Whether or to sort in ascending order.</param>
        public SortExpr(string varName, Expr source, Expr filter, bool isAsc)
        {
            _varName = varName;
            _isAsc = isAsc;
            _source = source;
            _filter = filter;
        }


        /// <summary>
        /// Evaluate the linq expression.
        /// </summary>
        /// <returns></returns>
        public override object Evaluate()
        {
            var array = _source.Evaluate();
            List<object> items = (array as LArray).Raw;

            // 1. Basic datatypes string, bool, number, date.
            if (!(_filter is MemberAccessExpr))
            {
                items.Sort(delegate(object x, object y)
                {
                    // Check for null for either x y and same values.
                    if (x == null)
                    {
                        if (y == null) return 0;
                        return -1;
                    }
                    if (y == null) return 1;
                    if (x == y) return 0;

                    // Now do the actual comparison of values
                    int result = 0;
                    if (_isAsc)
                        result = ((IComparable)x).CompareTo(y);
                    else
                        result = ((IComparable)y).CompareTo(x);
                    return result;
                });
                return array;
            }

            // 2. Sort complex object
            //    Member access expression
            //    sort books by book.pages asc
            items.Sort(delegate(object x, object y)
            {
                // Check for null for either x y and same values.
                if (x == null)
                {
                    if (y == null) return 0;
                    return -1;
                }
                if (y == null) return 1;
                if (x == y) return 0;

                // Now do the actual comparison of values
                Ctx.Memory.SetValue(_varName, x);
                object a = _filter.Evaluate();

                Ctx.Memory.SetValue(_varName, y);
                object b = _filter.Evaluate();

                int result = 0;
                if (_isAsc)
                    result = ((IComparable)a).CompareTo(b);
                else
                    result = ((IComparable)b).CompareTo(a);
                return result;
            });
            Ctx.Memory.Remove(_varName);
            return array;
        }
    }
}