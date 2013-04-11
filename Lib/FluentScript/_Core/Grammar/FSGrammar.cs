using System.Collections.Generic;
using System.Linq;

namespace Fluentscript.Lib._Core.Grammar
{
    public class ExtendedPropsHelper
    {
        public static List<ExtendedProp> Parse(string extraProps)
        {
            var propsList = new List<ExtendedProp>();
            if (!string.IsNullOrEmpty(extraProps))
            {
                if (!extraProps.Contains(';'))
                    extraProps += ";";

                var props = extraProps.Split(';');
                foreach (var prop in props)
                {
                    if (!string.IsNullOrEmpty(prop))
                    {
                        var tokens = prop.Split(':');
                        var name = tokens[1];
                        var type = tokens[0];
                        var modifier = ";";
                        if (name.StartsWith("prop-"))
                        {
                            name = name.Substring(5);
                            modifier = " { get; set; }";
                        }
                        propsList.Add(new ExtendedProp() { Name = name, TypeName = type, PostModifier = modifier });
                    }
                }
            }
            return propsList;
        }
    }


    public class TokenSpec
    {
        public TokenSpec(int val, string kind, string text, string varname)
        {
            this.Value = val;
            this.Kind = kind;
            this.Text = text;
            this.VarName = varname;
            if(kind.StartsWith("Literal"))
            {
                var tokens = text.Split(':');
                this.Text = tokens[0];
                this.LiteralValue = tokens[1];
                this.IsLiteral = true;
            }
            this.GenerateObject = true;
        }


        public int Value { get; set; }
        public string LiteralValue { get; set; }
        public string Kind { get; set; }
        public string Text { get; set; }
        public string VarName { get; set; }
        public bool IsLiteral { get; set; }
        public bool GenerateObject { get; set; }
    }


    public class TypeSpec
    {
        public TypeSpec()
        {
            this.Props = new List<ExtendedProp>();
        }


        public TypeSpec(string className, int val, string name, string fullName, string varname, string hostDataType)
        {
            this.Value = val;
            this.VarName = varname;
            this.Props = new List<ExtendedProp>();
        }

        public bool Generate { get; set; }
        public int Value { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string VarName { get; set; }
        public string ClassName { get; set; }
        public string HostDataType { get; set; }
        public string Type { get; set; }
        public string ConstValue { get; set; }
        public string ExtraProps { get; set; }
        public List<ExtendedProp> Props { get; set; }
        public string ExtraTypeProps { get; set; }
        public List<ExtendedProp> TypeProps { get; set; }


        public void InitExtraProps()
        {
            this.TypeProps = ExtendedPropsHelper.Parse(this.ExtraTypeProps);
            this.Props = ExtendedPropsHelper.Parse(this.ExtraProps);
        }
    }


    public class ExtendedProp
    {
        public string TypeName;
        public string Name;
        public string PostModifier;
    }


    public class ExprSpec
    {
        public ExprSpec(string className, string name, string extends, string implements, 
            string token,  bool createPlugin, string pluginImplements, int precedence, bool isStatement, bool hasTerminator, 
            bool isBlock, bool hasLhsRhs, bool createNewSymScope, bool hasOperator, 
            string qualifiedName, string extraProps)
        {
            this.ClassName = className;
            this.Extends = extends;
            this.Implements = implements;
            this.Name = name;
            this.Token = token;
            this.CreatePlugin = createPlugin;
            this.IsStatement = isStatement;
            this.HasTerminator = hasTerminator;
            this.IsBlock  = isBlock;  
            this.HasLhsRhs = hasLhsRhs;
            this.Precedence = precedence;
            this.CreateNewSymScope = createNewSymScope;
            this.HasOperator      = hasOperator;
            this.ExtraProps = extraProps;
            this.QualifiedName = qualifiedName;
            this.PluginImplements = pluginImplements;
            this.InitExtendedProps();
        }


        public void InitExtendedProps()
        {
            this.Props = ExtendedPropsHelper.Parse(this.ExtraProps);
        }


        public bool IsAutoMatched()
        {
            if (string.IsNullOrEmpty(Token))
                return false;
            if (Token.Contains("@"))
                return false;
            if (IsStatement)
                return true;
            return false;
        }


        public string ClassName { get; set; }
        public string Extends { get; set; }
        public string Implements { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public int Precedence { get; set; }
        public bool   IsStatement { get; set; }
        public bool HasTerminator { get; set; }
        public bool CreatePlugin { get; set; }
        public string QualifiedName { get; set; }
        public string PluginImplements { get; set; }
        public bool   IsBlock           { get; set; }
        public bool   HasLhsRhs         { get; set; }
        public bool   CreateNewSymScope { get; set; }
        public bool   HasOperator       { get; set; }
        public string ExtraProps        { get; set; }
        public List<ExtendedProp> Props { get; set; } 
    }


    public class FSGrammarSpec
    {
        public List<TokenSpec> TokenSpecs { get; set; }
        public List<ExprSpec> ExprSpecs { get; set; }
        public List<TypeSpec> TypeSpecs { get; set; }

        public void Setup()
        {
            this.TokenSpecs = new List<TokenSpec>()
            {
                new TokenSpec( 100,  "Keyword",      "var"      ,    "Var"       ),
                new TokenSpec( 101,  "Keyword",      "if"       ,    "If"        ),
                new TokenSpec( 102,  "Keyword",      "else"     ,    "Else"      ),
                new TokenSpec( 103,  "Keyword",      "break"    ,    "Break"     ),
                new TokenSpec( 104,  "Keyword",      "continue" ,    "Continue"  ),
                new TokenSpec( 105,  "Keyword",      "for"      ,    "For"       ),
                new TokenSpec( 106,  "Keyword",      "while"    ,    "While"     ),
                new TokenSpec( 107,  "Keyword",      "function" ,    "Function"  ),
                new TokenSpec( 108,  "Keyword",      "return"   ,    "Return"    ),
                new TokenSpec( 109,  "Keyword",      "new"      ,    "New"       ),
                new TokenSpec( 110,  "Keyword",      "try"      ,    "Try"       ),
                new TokenSpec( 111,  "Keyword",      "catch"    ,    "Catch"     ),
                new TokenSpec( 112,  "Keyword",      "throw"    ,    "Throw"     ),
	            new TokenSpec( 113,  "Keyword",      "in"    	,    "In"     	 ),
	            new TokenSpec( 114,  "Keyword",      "run"    	,    "Run"     	 ),
	            new TokenSpec( 115,  "Keyword",      "then"     ,    "Then"      ),

                new TokenSpec( 300,	 "Symbol",	     "+",	        "Plus"               	),
                new TokenSpec( 301,	 "Symbol",	     "-",	        "Minus"              	),
                new TokenSpec( 302,	 "Symbol",	     "*",	        "Multiply"           	),
                new TokenSpec( 303,	 "Symbol",	     "/",	        "Divide"             	),
                new TokenSpec( 304,	 "Symbol",	     "%",	        "Percent"             	),
	            new TokenSpec( 305,	 "Symbol",	     "<",	        "LessThan" 				),
                new TokenSpec( 306,	 "Symbol",	     "<=",	        "LessThanOrEqual" 		),
                new TokenSpec( 307,	 "Symbol",	     ">",	        "MoreThan" 				),
                new TokenSpec( 308,	 "Symbol",	     ">=",	        "MoreThanOrEqual" 		),
                new TokenSpec( 309,	 "Symbol",	     "==",	        "EqualEqual" 			),
                new TokenSpec( 310,	 "Symbol",	     "!=",	        "NotEqual" 				),
                new TokenSpec( 311,	 "Symbol",	     "&&",	        "LogicalAnd" 			),
                new TokenSpec( 312,	 "Symbol",	     "||",	        "LogicalOr" 			),
                new TokenSpec( 313,	 "Symbol",	     "!",	        "LogicalNot" 			),
                new TokenSpec( 314,	 "Symbol",	     "?",	        "Question"	 			),
                new TokenSpec( 315,	 "Symbol",	     "++",	        "Increment"				),
                new TokenSpec( 316,	 "Symbol",	     "--",	        "Decrement"				),
                new TokenSpec( 317,	 "Symbol",	     "+=",	        "IncrementAdd" 			),
                new TokenSpec( 318,	 "Symbol",	     "-=",	        "IncrementSubtract"		),
                new TokenSpec( 319,	 "Symbol",	     "*=",	        "IncrementMultiply"		),
                new TokenSpec( 320,	 "Symbol",	     "/=",	        "IncrementDivide" 		),
                new TokenSpec( 321,	 "Symbol",	     "{",	        "LeftBrace"				),
                new TokenSpec( 322,	 "Symbol",	     "}",	        "RightBrace" 			),
                new TokenSpec( 323,	 "Symbol",	     "(",	        "LeftParenthesis" 		),
                new TokenSpec( 324,	 "Symbol",	     ")",	        "RightParenthesis" 		),
                new TokenSpec( 325,	 "Symbol",	     "[",	        "LeftBracket" 			),
                new TokenSpec( 326,	 "Symbol",	     "]",	        "RightBracket" 			),
                new TokenSpec( 327,	 "Symbol",	     ";",	        "Semicolon"				),
                new TokenSpec( 328,	 "Symbol",	     ",",	        "Comma"					),
                new TokenSpec( 329,	 "Symbol",	     ".",	        "Dot" 					),
                new TokenSpec( 330,	 "Symbol",	     ":",	        "Colon"					),
                new TokenSpec( 331,	 "Symbol",	     "=",	        "Assignment" 			),
                new TokenSpec( 332,	 "Symbol",	     "$",	        "Dollar" 				),
                new TokenSpec( 333,	 "Symbol",	     "@",	        "At" 					),
                new TokenSpec( 334,	 "Symbol",	     "#",	        "Pound"					),
                new TokenSpec( 335,	 "Symbol",	     "|",	        "Pipe" 					),
                new TokenSpec( 336,	 "Symbol",	     "\\",	        "BackSlash"				),
                
                new TokenSpec( 400,	 "LiteralOther",	"eof:\"eof\"",	        "EndToken"				    ),
                new TokenSpec( 401,	 "LiteralOther",	"unknown:\"unknown\"",	"Unknown"				),
                new TokenSpec( 402,	 "LiteralOther",	"multi:\"multi\"",	    "Multi"				    ) { GenerateObject =  false },

                new TokenSpec( 200,   "LiteralBool",   "true:true"                   ,  "True"),
                new TokenSpec( 201,   "LiteralBool",   "false:false"                 ,  "False"),
                new TokenSpec( 202,   "LiteralOther",  "null:null"                   ,  "Null"),
                new TokenSpec( 203,   "LiteralOther",  " :\"\""                      ,  "WhiteSpace"),
                new TokenSpec( 204,   "LiteralOther",  "newline:\"newline\""         ,  "NewLine"),
                new TokenSpec( 205,   "LiteralOther",  "comment_sl:\"comment_sl\""   ,  "CommentSLine"),
                new TokenSpec( 206,   "LiteralOther",  "comment_ml:\"comment_ml\""   ,  "CommentMLine"),
                new TokenSpec( 207,   "LiteralIdent",  "na:\"na\""   ,  "Ident"	        ) { GenerateObject = false } ,
                new TokenSpec( 208,   "LiteralOther",  "na:\"na\""   ,  "LiteralBool"	) { GenerateObject = false } ,
                new TokenSpec( 209,   "LiteralOther",  "na:\"na\""   ,  "LiteralDate"	) { GenerateObject = false } ,
                new TokenSpec( 210,   "LiteralOther",  "na:\"na\""   ,  "LiteralDay"	) { GenerateObject = false } ,
                new TokenSpec( 211,   "LiteralOther",  "na:\"na\""   ,  "LiteralNumber"	) { GenerateObject = false } ,
                new TokenSpec( 212,   "LiteralOther",  "na:\"na\""   ,  "LiteralString"	) { GenerateObject = false } ,
                new TokenSpec( 213,   "LiteralOther",  "na:\"na\""   ,  "LiteralTime"	) { GenerateObject = false } ,
                new TokenSpec( 214,   "LiteralOther",  "na:\"na\""   ,  "LiteralVersion") { GenerateObject = false } ,
                new TokenSpec( 215,   "LiteralOther",  "na:\"na\""   ,  "LiteralOther"	) { GenerateObject = false } ,
                
            };

            this.ExprSpecs = new List<ExprSpec>()
            {
                new ExprSpec(className : "ArrayExpr", 			name: "Array", 		     extends: "IndexableExpr",          implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "List<Expr>:Exprs"),
                new ExprSpec(className : "AnyOfExpr", 			name: "AnyOf", 		     extends: "Expr",                   implements: "IParameterExpression", token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:CompareExpr;List<Expr>:prop-ParamListExpressions;List<object>:prop-ParamList"),
                new ExprSpec(className : "AssignExpr", 			name: "Assign", 		 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:VarExp;Expr:ValueExp;bool:IsDeclaration"),
                new ExprSpec(className : "AssignMultiExpr", 	name: "AssignMulti", 	 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "List<AssignExpr>:Assignments"),
                new ExprSpec(className : "BinaryExpr", 			name: "Binary", 		 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: true ,  createNewSymScope: false,  hasOperator: true ,  qualifiedName: "return \"\";",                                                            extraProps: null),
                new ExprSpec(className : "CompareExpr", 		name: "Compare", 		 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: true ,  createNewSymScope: false,  hasOperator: true ,  qualifiedName: "return \"\";",                                                            extraProps: null),
                new ExprSpec(className : "ConditionExpr", 		name: "Condition", 		 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: true ,  createNewSymScope: false,  hasOperator: true ,  qualifiedName: "return \"\";",                                                            extraProps: null),
                new ExprSpec(className : "ConstantExpr", 		name: "Constant", 		 extends: "ValueExpr",              implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: null),
                new ExprSpec(className : "DayExpr", 		    name: "Day", 		     extends: "ValueExpr",              implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "string:Name;string:Time"),
                new ExprSpec(className : "DurationExpr", 		name: "Duration", 		 extends: "ValueExpr",              implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "string:Duration;string:Mode"),
                new ExprSpec(className : "DateExpr", 		    name: "Date", 		     extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "int:Month;int:Day;int:Year;string:Time"),
                new ExprSpec(className : "DateRelativeExpr", 	name: "DateRelative",    extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "int:Month;int:DayOfTheWeek;string:RelativeDay"),
                new ExprSpec(className : "FunctionCallExpr", 	name: "FunctionCall", 	 extends: "Expr",                   implements: "IParameterExpression", token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return this.NameExp != null ? this.NameExp.ToQualifiedName() : \"\";",    extraProps: "Expr:NameExp;List<Expr>:prop-ParamListExpressions;List<object>:prop-ParamList;FunctionExpr:Function;bool:RetainEvaluatedParams;bool:IsScopeVariable"),
                new ExprSpec(className : "FunctionExpr", 		name: "Function", 		 extends: "BlockExpr",              implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "FunctionMetaData:Meta;long:ExecutionCount;long:ErrorCount;bool:HasReturnValue;object:ReturnValue;List<object>:ArgumentValues;bool:ContinueRunning"),
                new ExprSpec(className : "IndexExpr", 			name: "Index", 			 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:IndexExp;Expr:VarExp;bool:IsAssignment"),
                new ExprSpec(className : "InterpolatedExpr", 	name: "Interpolated", 	 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "List<Expr>:Expressions"),
                new ExprSpec(className : "ListCheckExpr", 		name: "ListCheck", 		 extends: "Expr",                   implements: "",                     token : ""	        ,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:NameExp"),
                new ExprSpec(className : "MapExpr", 			name: "Map", 			 extends: "IndexableExpr",          implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "List<Tuple<string,Expr>>:Expressions"),
                new ExprSpec(className : "MemberAccessExpr", 	name: "MemberAccess", 	 extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return this.VarExp.ToQualifiedName() + \".\" + this.MemberName;",         extraProps: "string:MemberName;Expr:VarExp;bool:IsAssignment"),
                new ExprSpec(className : "NamedParameterExpr", 	name: "NamedParameter",  extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return this.Name;",                                                       extraProps: "string:Name;Expr:Value;int:Pos"),
                new ExprSpec(className : "NegateExpr", 			name: "Negate", 		 extends: "VariableExpr",           implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:Expression"),
                new ExprSpec(className : "NewExpr", 			name: "New", 			 extends: "Expr",                   implements: "IParameterExpression", token : "new"		,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "string:TypeName;List<Expr>:prop-ParamListExpressions;List<object>:prop-ParamList"),
                new ExprSpec(className : "ParameterExpr", 		name: "Parameter", 		 extends: "Expr",                   implements: "IParameterExpression", token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "FunctionMetaData:Meta;List<Expr>:prop-ParamListExpressions;List<object>:prop-ParamList;"),
                new ExprSpec(className : "RunExpr", 		    name: "Run", 		     extends: "Expr",                   implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "string:FuncName;string:Mode;Expr:FuncCallOnAfterExpr;Expr:FuncCallExpr"),
                new ExprSpec(className : "TableExpr", 			name: "Table", 		     extends: "IndexableExpr",          implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "List<string>:Fields"),
                new ExprSpec(className : "UnaryExpr", 			name: "Unary", 			 extends: "VariableExpr",           implements: "",                     token : ""			,  createPlugin : false,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Operator:Op;Expr:Expression;double:Increment"),
                new ExprSpec(className : "BreakExpr", 			name: "Break", 			 extends: "Expr",                   implements: "",                     token : "break"	    ,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: true ,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: null),
                new ExprSpec(className : "ContinueExpr", 		name: "Continue", 		 extends: "Expr",                   implements: "",                     token : "continue"	,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: true ,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: null),
                new ExprSpec(className : "ForEachExpr", 		name: "ForEach", 		 extends: "WhileExpr",              implements: "",                     token : "for"		,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "string:VarName;Expr:SourceExpr;string:SourceName"),
                new ExprSpec(className : "ForExpr", 			name: "For", 			 extends: "WhileExpr",              implements: "",                     token : "for"		,  createPlugin : true ,  pluginImplements: "",                 precedence: 2,  isStatement: true,   hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:Start;Expr:Increment"),
                new ExprSpec(className : "FunctionDeclareExpr", name: "FunctionDeclare", extends: "Expr",                   implements: "",                     token : "function"	,  createPlugin : true ,  pluginImplements: "IParserCallbacks", precedence: 1,  isStatement: true,   hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "FunctionExpr:Function"),
                new ExprSpec(className : "IfExpr", 				name: "If", 			 extends: "ConditionalBlockExpr",   implements: "",                     token : "if"		,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "BlockExpr:Else"),
                new ExprSpec(className : "LambdaExpr", 			name: "Lambda", 		 extends: "Expr",                   implements: "",                     token : "function"	,  createPlugin : true ,  pluginImplements: "IParserCallbacks", precedence: 1,  isStatement: true,   hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "FunctionExpr:Expr"),
                new ExprSpec(className : "ReturnExpr", 			name: "Return", 		 extends: "Expr",                   implements: "",                     token : "return"	,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: true ,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:Exp"),
                new ExprSpec(className : "ThrowExpr", 			name: "Throw", 			 extends: "Expr",                   implements: "",                     token : "throw"	    ,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: true ,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "Expr:Exp"),
                new ExprSpec(className : "TryCatchExpr", 		name: "TryCatch", 		 extends: "Expr",                   implements: "IBlockExpr",           token : "try"		,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "string:ErrorName;List<Expr>:prop-Statements;BlockExpr:Catch"),
                new ExprSpec(className : "WhileExpr", 			name: "While", 			 extends: "ConditionalBlockExpr",   implements: "ILoop",                token : "while"	    ,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: true,   hasTerminator: false,  isBlock: true ,  hasLhsRhs: false,  createNewSymScope: true ,  hasOperator: false,  qualifiedName: "return \"\";",                                                            extraProps: "bool:prop-DoBreakLoop;bool:prop-DoContinueLoop;bool:prop-DoContinueRunning"),
                new ExprSpec(className : "VariableExpr", 		name: "Variable", 		 extends: "Expr",                   implements: "",                     token : "@ident"	,  createPlugin : true ,  pluginImplements: "",                 precedence: 1,  isStatement: false,  hasTerminator: false,  isBlock: false,  hasLhsRhs: false,  createNewSymScope: false,  hasOperator: false,  qualifiedName: "return this.Name;",                                                       extraProps: "string:Name"),
            };


            this.TypeSpecs = new List<TypeSpec>()
            {
                new TypeSpec(){ ClassName = "Array",     Generate = true,  ConstValue = "Array",     Type= "",  Name = "array",       FullName = "sys.array",     HostDataType = "IList",                         Value = 1 ,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "Bool",      Generate = true,  ConstValue = "Bool",      Type= "",  Name = "bool",        FullName = "sys.bool",      HostDataType = "bool",                          Value = 2 ,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "Class",     Generate = true,  ConstValue = "LClass",    Type= "", Name = "class",       FullName = "ext.class",     HostDataType = "object",                        Value = 3 ,    ExtraTypeProps = "Type:DataType" },
                new TypeSpec(){ ClassName = "Date",      Generate = true,  ConstValue = "Date",      Type= "",  Name = "datetime",    FullName = "sys.datetime",  HostDataType = "DateTime",                      Value = 4 ,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "DayOfWeek", Generate = true,  ConstValue = "DayOfWeek", Type= "",  Name = "dayofweek",   FullName = "sys.dayofweek", HostDataType = "DayOfWeek",                     Value = 5 ,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "Function",  Generate = true,  ConstValue = "Function",  Type= "",  Name = "function",    FullName = "ext.function",  HostDataType = "object",                        Value = 6 ,   ExtraTypeProps = "LType:Parent" },
                new TypeSpec(){ ClassName = "Map",       Generate = true,  ConstValue = "Map",       Type= "",  Name = "map",         FullName = "sys.map",       HostDataType = "IDictionary<string, object>",   Value = 7 ,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "Module",    Generate = true,  ConstValue = "Module",    Type= "",  Name = "module",      FullName = "ext.module",    HostDataType = "object",                        Value = 8 ,   ExtraTypeProps = "" },
                new TypeSpec(){ ClassName = "Null",      Generate = true,  ConstValue = "Null",      Type= "",  Name = "null",        FullName = "sys.null",      HostDataType = "object",                        Value = 9 ,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "Number",    Generate = true,  ConstValue = "Number",    Type= "",  Name = "number",      FullName = "sys.number",    HostDataType = "double",                        Value = 10,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "Object",    Generate = false, ConstValue = "Object",    Type= "",  Name = "object",      FullName = "sys.object",    HostDataType = "object",                        Value = 11,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "String",    Generate = true,  ConstValue = "String",    Type= "",  Name = "string",      FullName = "sys.string",    HostDataType = "string",                        Value = 12,   ExtraTypeProps = ""},
                new TypeSpec(){ ClassName = "Table",     Generate = true,  ConstValue = "Table",     Type= "",  Name = "table",       FullName = "sys.table",     HostDataType = "IList",                         Value = 1 ,   ExtraTypeProps = "", ExtraProps = "List<string>:Fields"},
                new TypeSpec(){ ClassName = "Time",      Generate = true,  ConstValue = "Time",      Type= "",  Name = "time",        FullName = "sys.time",      HostDataType = "TimeSpan",                      Value = 13,   ExtraTypeProps = ""},                
                new TypeSpec(){ ClassName = "Unit",      Generate = true,  ConstValue = "Unit",      Type= "",  Name = "unit",        FullName = "sys.unit",      HostDataType = "double",                        Value = 14,   ExtraTypeProps = "", ExtraProps = "double:prop-BaseValue;string:prop-Group;string:prop-SubGroup"},
            };
            foreach (var typespec in this.TypeSpecs)
            {
                typespec.InitExtraProps();
            }
        }
    }
}
