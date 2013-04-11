using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
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
            _terminators[TokenBuilder.ToIdentifier("asc")] = true;
            _terminators[TokenBuilder.ToIdentifier("desc")] = true;
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
                if (_source.IsNodeType(NodeTypes.SysVariable))
                {
                    _variableName = ((VariableExpr)_source).Name;
                }
                else if(_source.IsNodeType(NodeTypes.SysMemberAccess))
                {
                    _variableName = ((MemberAccessExpr)_source).MemberName;
                    _variableName = _variableName.Substring(0, _variableName.Length - 1);
                }
                else if (_source.IsNodeType(NodeTypes.SysArray))
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
            this.Nodetype = "FSExtSort";
            _varName = varName;
            _isAsc = isAsc;
            _source = source;
            _filter = filter;
        }


        /// <summary>
        /// Whether or not this is of the node type supplied.
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public override bool IsNodeType(string nodeType)
        {
            if (nodeType == "FSExtSort")
                return true;
            return base.IsNodeType(nodeType);
        }


        /// <summary>
        /// Evaluate the linq expression.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            var obj = _source.Evaluate(visitor);

            // Check 1: not null
            ExceptionHelper.NotNull(this, obj, "sort");

            // Check 2: either array or table
            var lobj = obj as LObject;
            if (lobj.Type != LTypes.Array && lobj.Type != LTypes.Table)
                ExceptionHelper.BuildRunTimeException(this, "Sort is only supported for lists(arrays) and tables");

            var items = lobj.GetValue() as List<object>;

            // 1. Basic datatypes string, bool, number, date.
            if (!(_filter != null && _filter.IsNodeType(NodeTypes.SysMemberAccess)))
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
                        result = CompareObjects(x, y);
                    else
                        result = CompareObjects(y, x);
                    return result;
                });
                return lobj;
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
                object a = _filter.Evaluate(visitor);

                Ctx.Memory.SetValue(_varName, y);
                object b = _filter.Evaluate(visitor);

                int result = 0;
                if (_isAsc)
                    result = CompareObjects(a, b);
                else
                    result = CompareObjects(b, a);
                return result;
            });
            Ctx.Memory.Remove(_varName);
            return lobj;
        }


        private int CompareObjects(object x, object y)
        {
            var a = ((LObject)x).GetValue();
            var b = ((LObject)y).GetValue();
            var result = ((IComparable)a).CompareTo(b);
            return result;
        }
    }
}