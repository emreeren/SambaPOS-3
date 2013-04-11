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
    // Provides ability to check variable types and convert from one type to another.
        
    // 1. Supported types = "number", "yesno", "date", "time", "string"    
    // 2. Supported functions :
    // 3. You can replace the the "<type>" below with any of the supported types above.
     
    //  NAME             EXAMPLE                            RESULT
    //  is_<type>        is_number( 123 )                   true
    //  is_<type>        is_number( '123' )                 false
    //  is_<type>_like   is_number_like( '123' )            true
    //  to_<type>        to_number( '123' )                 123
    
    //  is_<type>        is_bool( true )                   true
    //  is_<type>        is_bool( 'true' )                 false
    //  is_<type>_like   is_bool_like( 'true' )            true
    //  to_<type>        to_bool( 'true' )                 true
    
    //  is_<type>        is_date( new Date(2012, 9, 10 )    true
    //  is_<type>        is_date( '9/10/2012' )             false
    //  is_<type>_like   is_date_like( '9/10/2012' )        true
    //  to_<type>        to_date( '9/10/2012' )             Date(2012, 9, 10)
    //  to_<type>        to_time( '8:30' )                  Time(8, 30, 0)
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling swapping of variable values. swap a and b.
    /// </summary>
    public class TypeOperationsPlugin : ExprPlugin
    {
        private IDictionary<string, string> _functionToTypeMap;


        /// <summary>
        /// Intialize.
        /// </summary>
        public TypeOperationsPlugin()
        {
            this.IsStatement = false;
            this.IsAutoMatched = true;
            _functionToTypeMap = new Dictionary<string, string>();
            var types = new string[] { "number", "bool", "date", "time", "string" };
            var functionnames = new List<string>();

            // create all the supported functions: e.g. for "number" we have:
            // 1. "is_number"
            // 2. "is_number_like"
            // 3. "to_number"
            foreach (var type in types)
            {
                _functionToTypeMap["is_" + type] = type;
                _functionToTypeMap["is_" + type + "_like"] = type;
                _functionToTypeMap["to_" + type] = type;
            }
            this.StartTokens = _functionToTypeMap.Keys.ToArray();
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "typeof <expression>";
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
                    "is_number( 123 )",
                    "is_number_like( '123' )",
                    "to_number( '123' )",
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var functionName = _tokenIt.NextToken.Token.Text;
            
            // Move to next token. possibly a "("
            _tokenIt.Advance();
            var expectParenthesis = _tokenIt.NextToken.Token.Type == TokenTypes.LeftParenthesis;
            if (expectParenthesis)
                _tokenIt.Advance();

            // 1. Get the expression.
            var exp = _parser.ParseExpression(Terminators.ExpFlexibleEnd, true, false, true, true, false);            

            // 2. map the function name to the expression.
            var destinationType = _functionToTypeMap[functionName];

            // 3. determine if converting or just checking.
            var isConverting = functionName.StartsWith("to"); 
            
            // 4. checking for possible conversion? is_number_like( "123" )
            var isConversionCheck = functionName.Contains("like");

            if (expectParenthesis)
                _tokenIt.Expect(Tokens.RightParenthesis);

            return new TypeOperationsExpr(isConverting, isConversionCheck, destinationType, exp);
        }
    }



    /// <summary>
    /// Variable expression data
    /// </summary>
    public class TypeOperationsExpr : Expr
    {        
        private Expr _exp;
        private bool _isConversion;
        private bool _isConversionCheck;
        private string _destinationType;
        private static int CONVERT_MODE_DIRECT   = 0;
        private static int CONVERT_MODE_TOSTRING = 1;
        private static int CONVERT_MODE_LAMBDA   = 2;
        private static int CONVERT_MODE_NA       = 3;
        

        private static Dictionary<string, ConvertSpec> _conversionLookup = new Dictionary<string, ConvertSpec>();
        private static List<ConvertSpec> _conversionSpecs = new List<ConvertSpec>()
        {
	        // 				source		dest	   can change		case sensitive	convert mode	regex	handler,  	allowedvalues
	        new ConvertSpec("string",	"string" , true ,			true , 		  	CONVERT_MODE_DIRECT,  	"",  	null, 		                                null),
	        new ConvertSpec("string",	"bool"   , true ,			false, 			CONVERT_MODE_LAMBDA, 	"",  	ConversionHelper.Convert_String_To_Bool,    null),
	        new ConvertSpec("string",	"number" , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	ConversionHelper.Convert_String_To_Number,  null),
	        new ConvertSpec("string",	"date"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	ConversionHelper.Convert_String_To_Date,    null),
	        new ConvertSpec("string",	"time"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	ConversionHelper.Convert_String_To_Time,    null),
	        
            new ConvertSpec("bool"  ,	"string" , true ,			false, 			CONVERT_MODE_TOSTRING,  "",  	null, 		                                null),
	        new ConvertSpec("bool"  ,	"bool"   , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                                null),
	        new ConvertSpec("bool"  ,	"number" , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	ConversionHelper.Convert_Bool_To_Number,    null),
	        new ConvertSpec("bool"  ,	"date"   , false,			false, 			CONVERT_MODE_NA,     	"",  	null, 		                                null),
	        new ConvertSpec("bool"  ,	"time"   , false,			false, 			CONVERT_MODE_NA,     	"",  	null, 		                                null),
	        
            new ConvertSpec("number",	"string" , true ,			false, 			CONVERT_MODE_TOSTRING,  "",  	null, 		                                null),
	        new ConvertSpec("number",	"bool"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	ConversionHelper.Convert_Number_To_Bool,    null),
	        new ConvertSpec("number",	"number" , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                                null),
	        new ConvertSpec("number",	"date"   , false,			false, 			CONVERT_MODE_NA,    	"",  	null, /*Convert_Number_To_Date*/ 	        null),
	        new ConvertSpec("number",	"time"   , false,			false, 			CONVERT_MODE_NA,    	"",  	null, /*Convert_Number_To_Time*/            null),
	        
            new ConvertSpec("date"  ,	"string" , true ,			false, 			CONVERT_MODE_LAMBDA,    "",  	ConversionHelper.Convert_Date_To_String,    null),
	        new ConvertSpec("date"  ,	"bool"   , false,			false, 			CONVERT_MODE_NA,     	"",  	null, 		                                null),
	        new ConvertSpec("date"  ,	"number" , false ,			false, 			CONVERT_MODE_NA,  	    "",  	null, /*Convert_Date_To_Number*/            null),
	        new ConvertSpec("date"  ,	"date"   , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                                null),
	        new ConvertSpec("date"  ,	"time"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	ConversionHelper.Convert_Date_To_Time,      null),
	        
            new ConvertSpec("time"  ,	"string" , true ,			false, 			CONVERT_MODE_LAMBDA,    "",  	ConversionHelper.Convert_Time_To_String,    null),
	        new ConvertSpec("time"  ,	"bool"   , false,			false, 			CONVERT_MODE_NA,  	    "",  	null, 		                                null),
	        new ConvertSpec("time"  ,	"number" , false ,			false, 			CONVERT_MODE_NA,  	    "",  	null, /*Convert_Time_To_Number*/            null),
	        new ConvertSpec("time"  ,	"date"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	ConversionHelper.Convert_Time_To_Date,      null),
	        new ConvertSpec("time"  ,	"time"   , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                                null),
        };


        /// <summary>
        /// Initalize lookups
        /// </summary>
        static TypeOperationsExpr()
        {
            foreach(var spec in _conversionSpecs)
                _conversionLookup[spec.LookupKey()] = spec;
        }


        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="isConversion">Whether or not to convert from exp to destination type</param>
        /// <param name="isConversionCheck">Whether or to TEST the conversion from exp to destination type</param>
        /// <param name="exp">The expression to check or convert</param>
        /// <param name="destinationType">The destination type</param>
        public TypeOperationsExpr(bool isConversion, bool isConversionCheck, string destinationType, Expr exp)
        {
            _exp = exp;
            _isConversion = isConversion;
            _destinationType = destinationType;
            _isConversionCheck = isConversionCheck;
        }


        /// <summary>
        /// Evaluate the type check/conversion operation.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            if (_isConversion)
                return ConvertValue(visitor);
            else if (_isConversionCheck)
            {
                var result = TryConvertValue(visitor);
                return result;
            }
            return CheckExplicitType(visitor);
        }


        /// <summary>
        /// Used for function calls like "is_number(123)"
        /// </summary>
        /// <returns></returns>
        private object CheckExplicitType(IAstVisitor visitor)
        {
            var val = _exp.Evaluate(visitor);
            if (val == null || val == LObjects.Null)
                return new LBool(false);

            var lobj = val as LObject;
            var result = false;
            if (     _destinationType == "string" ) result = lobj.Type == LTypes.String;
            else if (_destinationType == "number" ) result = lobj.Type == LTypes.Number;
            else if (_destinationType == "bool"   ) result = lobj.Type == LTypes.Bool;
            else if (_destinationType == "date"   ) result = lobj.Type == LTypes.Date;
            else if (_destinationType == "time"   ) result = lobj.Type == LTypes.Time;
            else if (_destinationType == "list"   ) result = lobj.Type == LTypes.Array;
            else if (_destinationType == "map"    ) result = lobj.Type == LTypes.Map;
            return new LBool(result);
        }

        /// <summary>
        /// Used for function calls like "to_number( '123' )";
        /// </summary>
        /// <returns></returns>
        private object TryConvertValue(IAstVisitor visitor)
        {
            var val = _exp.Evaluate(visitor);
            if (val == null)
                return new LBool(false);
            var canConvert = false;
            try
            {
                var result = DoConvertValue(_destinationType, val, false);
                if (result != LObjects.Null)
                    canConvert = true;
            }
            catch (Exception)
            {
            }
            return new LBool(canConvert);
        }


        /// <summary>
        /// Used for function calls like "to_number( '123' )";
        /// </summary>
        /// <returns></returns>
        private object ConvertValue(IAstVisitor visitor)
        {
            var val = _exp.Evaluate(visitor);
            if (val == null)
                return LObjects.Null;
            var result = DoConvertValue(_destinationType, val, true);
            return result;
        }


        private object DoConvertValue(string destinationType, object val, bool handleError)
        {
            var lobj = (LObject)val;

            // get the source type
            var sourceType = GetTypeName(val);
            var key = sourceType + "-" + destinationType;

            // 1. Check if conversion even exists
            if (!_conversionLookup.ContainsKey(key)) return LObjects.Null;

            // 2. Get the conversion and check it source can be converted to dest.
            var spec = _conversionLookup[key];
            if (!spec.CanChange) return LObjects.Null;

            // 3a. See if there can be a direct conversion.
            if (spec.ConvertMode == CONVERT_MODE_DIRECT)
                return val;

            // 3b. ToString
            if (spec.ConvertMode == CONVERT_MODE_TOSTRING)
                return new LString(lobj.GetValue().ToString());

            // 3c. Conversion method
            object result = null;
            if (spec.ConvertMode == CONVERT_MODE_LAMBDA)
            {
                if (!handleError)
                    result = spec.Handler(spec, val);
                else
                {
                    try
                    {
                        result = spec.Handler(spec, val);
                    }
                    catch (Exception)
                    {
                        throw BuildRunTimeException("Unable to convert : " + val.ToString() + " to " + destinationType);
                    }
                }
            }
            return result;
        }


        private static string GetTypeName(object val)
        {
            var typename = ((LObject)val).Type.Name;
            if (typename == LTypes.Date.Name)
                typename = "date";
            return typename;
        }
    }
}
