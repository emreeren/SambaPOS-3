using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Supports representing data in a table/csv like format
    // 1. The top most row must the the header names without spaces
    // 2. The header names must be separated by "|"
    // 3. The columns in the data rows must be separated by either "|" or ","
    
    set books1 = [  
                     name	      |	 pages   |  artist
                     'batman'     |	 110     |  'john'
                     'xmen'       |	 120     |  'lee'
                     'daredevil'  |	 140     |  'maleev'
                 ];
     
     
     set books2 = [  
                     name	      |	 pages   |  artist
                     'batman'     ,	 110     ,  'john'
                     'xmen'       ,	 120     ,  'lee'
                     'daredevil'  ,	 140     ,  'maleev'
                  ];
 
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling swapping of variable values. swap a and b.
    /// </summary>
    public class RecordsPlugin : ExprPlugin
    {        
        private Dictionary<Token, bool> _endTokens;


        /// <summary>
        /// Intialize.
        /// </summary>
        public RecordsPlugin()
        {
            _endTokens = new Dictionary<Token, bool>();
            _endTokens[Tokens.Pipe] = true;
            _endTokens[Tokens.Comma] = true;
            _endTokens[Tokens.NewLine] = true;
            _endTokens[Tokens.RightBracket] = true;
            this.StartTokens = new string[] { "[" };
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "'[' <id> ( '|' <id> )* <newline> ( <expression> ( ( ',' | '|' ) <expression> )* <newline> )* ']'";
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
                    "[ name | age \r\n 'john', 21\r\n 'fred', 20 ]"
                };
            }
        }


        /// <summary>
        /// Whether or not this plugin is applicable for the token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var next = _tokenIt.Peek().Token;
            if (!(next.Kind == TokenKind.Ident)) return false;

            // Start of csv like table is :
            // [ name |
            var next2 = _tokenIt.Peek(2).Token;
            if (next2 == Tokens.Pipe )
                return true;

            return false;
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            /*
            [ 	
				name	 |	 pages	 | author
				'c#'	 |	 150	 | 'microsoft'
				'ruby'	 | 	 140	 | 'matz'
				'fluent' | 	 100	 | 'codehelix'
			];
             * [ 	
				name	 |	 pages	 | author
				'c#'	 ,	 150	 , 'microsoft'
				'ruby'	 , 	 140	 , 'matz'
				'fluent' , 	 100	 , 'codehelix'
			];
            */
            var startToken = _tokenIt.NextToken;
            // 1. Move past "["
            _tokenIt.Advance(1, true);

            var columnNames = new List<string>();
            var token = _tokenIt.NextToken.Token;
            while (!Token.IsNewLine(token) && !_tokenIt.IsEnded)
            {
                // Expect column name.
                string columnName = _tokenIt.ExpectId(false);
                columnNames.Add(columnName);

                _tokenIt.Advance(1, false);
                // "|" pipe to separate column names.
                if (_tokenIt.NextToken.Token == Tokens.Pipe)
                {
                    _tokenIt.Advance(1, false);
                }
                else if(_tokenIt.NextToken.Token != Tokens.NewLine)
                    throw _tokenIt.BuildSyntaxExpectedException("| or new line");

                token = _tokenIt.NextToken.Token;
            }
            if (_tokenIt.IsEnded)
                throw _tokenIt.BuildEndOfScriptException();

            // Hit new line?
            _tokenIt.Advance();

            var records = new List<List<Tuple<string, Expr>>>();
            var record = new List<Tuple<string, Expr>>();

            int colIndex = 0;
            Token firstColumnDataDelimiter = null;
            // Build up all the records.
            while (token != Tokens.RightBracket && !_tokenIt.IsEnded)
            {
                // 1. Get the column value: 'C#'
                var exp = _parser.ParseExpression(_endTokens, true, passNewLine: false);

                string colName = columnNames[colIndex];
                record.Add(new Tuple<string, Expr>(colName, exp));

                // 2. Is the next one a | or new line ?
                //_tokenIt.Advance(1, false);
                token = _tokenIt.NextToken.Token;

                // 3. If "|" or "," it separates expression/column values.
                if (firstColumnDataDelimiter == null && token == Tokens.Pipe || token == Tokens.Comma)
                    firstColumnDataDelimiter = token;

                if (token == firstColumnDataDelimiter)
                {
                    _tokenIt.Advance(1, false);
                    colIndex++;
                }

                // 4. If Newline, end of current record.
                else if (token == Tokens.NewLine)
                {
                    records.Add(record);
                    record = new List<Tuple<string, Expr>>();
                    _tokenIt.Advance();
                    colIndex = 0;
                }
                else if (token == Tokens.RightBracket)
                {
                    records.Add(record);
                    record = new List<Tuple<string, Expr>>();
                    colIndex = 0;
                }
                else
                {
                    throw _tokenIt.BuildSyntaxExpectedException("| or new line");
                }
                token = _tokenIt.NextToken.Token;                
            }            
            _tokenIt.Expect(Tokens.RightBracket);

            // Now finally build array of maps.
            List<Expr> array = new List<Expr>();
            foreach (var rec in records)
            {
                var item = Exprs.Map(rec, startToken);
                array.Add(item);
            }

            var arrayExp = Exprs.Array(array, startToken);
            return arrayExp;
        }
    }
}
