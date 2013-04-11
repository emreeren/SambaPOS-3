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
    // Linq plugin is a Light-weight and partial version of Linq style queries and comprehensions
    // NOTE: This has limited functionality as of this release.
    var books = [ 
				    { name: 'book 1', pages: 200, author: 'homey' },
				    { name: 'book 2', pages: 120, author: 'kdog' },
				    { name: 'book 3', pages: 140, author: 'homeslice' }
			    ];
     
    // Case 1: start with source <books> and system auto creates variable <book>
    var favorites = books where book.pages < 150 and book.author == 'kdog';
    
    // Case 2: using from <variable> in <source>
    var favorities = from book in books where book.pages < 150 and book.author == 'kdog';
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class LinqPlugin : ExprPlugin
    {        
        private static IDictionary<Token, bool> _terminators;


        private string _variableName;
        private Expr _source;
        private Expr _filter;


        static LinqPlugin()
        {
            _terminators = new Dictionary<Token, bool>();
            _terminators[TokenBuilder.ToIdentifier("order")] = true;
            _terminators[Tokens.Semicolon] = true;
            _terminators[Tokens.NewLine] = true;
            _terminators[Tokens.Comma] = true;
            _terminators[Tokens.RightParenthesis] = true;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public LinqPlugin()
        {
            this.StartTokens = new string[] { "$IdToken", "select", "from", "where" };
            this.Precedence = 1;
            this.IsContextFree = false;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "( <id> where <expression> ) | ( from <id> in <id> where <expression> )";
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
                    "books where book.pages < 150 and book.author == 'kdog'",    
                    "from book in books where book.pages < 150 and book.author == 'kdog'"
                };
            }
        }


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool CanHandle(Token token)
        {
            // 1. set favorites = select book from books where Pages < 100.
            // 2. set favorites = select from books where Pages < 100
            // 3. set favorites = from books where Pages < 100
            // 4. set favorites = books where Pages < 100
            var next = _tokenIt.Peek();

            if (string.Compare(token.Text, "select", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                if (next.Token.Kind == TokenKind.Symbol) 
                    return false;
                return true;
            }
            if (string.Compare(token.Text, "from", StringComparison.InvariantCultureIgnoreCase) == 0) return true;            
            if (string.Compare(next.Token.Text, "where", StringComparison.InvariantCultureIgnoreCase) == 0) return true;

            return false;
        }


        /// <summary>
        /// Handles sql like selections/comprehensions
        /// 1. set favorites = select book from books where Pages > 100.
        /// 2. set favorites = select from books where Pages > 100
        /// 3. set favorites = from books where Pages > 100
        /// 4. set favorites = books where Pages > 100
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // Handle only 2 for now.
            // 1. from book in books where book.Pages > 100 and author is 'gladwell'
            // 2. books where Pages > 100 and author is 'gladwell'
            ParseFrom();
            ParseWhere();

            if (_tokenIt.NextToken.Token.Text == "order")
                throw new NotSupportedException("order by not yet supported in Linq plugin for expressions");
            
            return new LinqExpr(_variableName, _source, _filter, null);
        }


        private void ParseFrom()
        {
            var token = _tokenIt.NextToken.Token;
            
            // 1. "from book in books where"            
            if (string.Compare(token.Text, "from", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                _tokenIt.Advance();
                _variableName = _tokenIt.ExpectId();
                _tokenIt.Expect(Tokens.In);
                _source = _parser.ParseIdExpression(null, null, false);
                return;
            }

            // 2. books where
            // In this case autocreate variable "book" using variable name.
            _source = _parser.ParseIdExpression(null, null, false);
            if (_source.IsNodeType(NodeTypes.SysVariable))
                _variableName = ((VariableExpr)_source).Name;
            else if (_source.IsNodeType(NodeTypes.SysMemberAccess))
                _variableName = ((MemberAccessExpr)_source).MemberName;

            // get "book" from "books".
            _variableName = _variableName.Substring(0, _variableName.Length - 1);
        }


        private void ParseWhere()
        {
            _tokenIt.ExpectIdText("where");
            _filter = _parser.ParseExpression(_terminators, enablePlugins: true, passNewLine: false);
        }
    }



    /// <summary>
    /// Expression to represent a Linq like query.
    /// </summary>
    public class LinqExpr : IndexableExpr
    {
        private Expr _source = null;
        private string _varName = null;
        private Expr _filter = null;
        private List<Expr> _sorts = null;


        /// <summary>
        /// Initialize
        /// </summary>
        public LinqExpr()
        {
            this.Nodetype = "FSExtLinq";
        }


        /// <summary>
        /// Initialize using values.
        /// </summary>
        /// <param name="varName">The name of the variable representing each item in the source</param>
        /// <param name="source">The data source being queried</param>
        /// <param name="filter">The filter to apply on the datasource</param>
        /// <param name="sorts">The sorting to apply after filtering.</param>
        public LinqExpr(string varName, Expr source, Expr filter, List<Expr> sorts)
        {
            this.Nodetype = "FSExtLinq";
            _varName = varName;
            _sorts = sorts;
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
            if (nodeType == "FSExtLinq")
                return true;
            return base.IsNodeType(nodeType);
        }


        /// <summary>
        /// Evaluate the linq expression.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            var sourceObj = _source.Evaluate(visitor) as LObject;
            
            // Check 1: null ?
            if (sourceObj == null) 
                throw BuildRunTimeException("Can not query the source list: it is null");

            // 1. get the name for debugging reasons.
            var name = _source.ToQualifiedName();

            // Check 2: datatype
            if (sourceObj.Type != LTypes.Array && sourceObj.Type != LTypes.Table)
                throw BuildRunTimeException("Can not query item " + name + " : it is not a list or table");

            var items = sourceObj.GetValue() as List<object>;
            var results = new List<object>();

            for (int ndx = 0; ndx < items.Count; ndx++)
            {
                var val = items[ndx];
                this.Ctx.Memory.SetValue(_varName, val);
                var isMatch = _filter.EvaluateAs<bool>(visitor);
                if (isMatch)
                {
                    results.Add(val);
                }
            }
            return new LArray(results);
        }
    }
}