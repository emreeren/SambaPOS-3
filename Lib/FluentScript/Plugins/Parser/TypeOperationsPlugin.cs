using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ComLib.Lang;
using ComLib.Lang.Helpers;


namespace ComLib.Lang.Extensions
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

        /// <summary>
        /// Internal class for specifying what conversions can take place.
        /// </summary>
        class ConvertSpec
        {
            /// <summary>
            /// Initialize
            /// </summary>
            /// <param name="sourceType">The source type</param>
            /// <param name="destType">The destination type</param>
            /// <param name="canChange">Whether or not the conversion change can occur with these types</param>
            /// <param name="isCaseSensitive">Whether or not the conversion is case sensitive.</param>
            /// <param name="convertMode">The mode of conversion.</param>
            /// <param name="regex">A regex pattern if mode involves regular expression checks.</param>
            /// <param name="allowedVals">A list of allowed values of source value</param>
            /// <param name="handler">A method to perform the conversion for complex conversions.</param>
            public ConvertSpec(string sourceType, string destType, bool canChange, bool isCaseSensitive, 
                               int convertMode, string regex, Func<ConvertSpec, object, object> handler, List<string> allowedVals)
            {
                SourceType = sourceType;
                DestType = destType;
                CanChange = canChange;
                IsCaseSensitive = isCaseSensitive;
                ConvertMode = convertMode;
                RegexPattern = regex;
                Handler = handler;
                AllowedVals = allowedVals;
            }


            /// <summary>
            /// Source type. e.g. "string"
            /// </summary>
	        public string SourceType 		{ get; set; }


            /// <summary>
            /// Destination type e.g. "bool"
            /// </summary>
	        public string DestType   		{ get; set; }


            /// <summary>
            /// Whether or not a change can take place.
            /// </summary>
	        public bool CanChange    		{ get; set; }


            /// <summary>
            /// Whether or not the change is case sensitive
            /// </summary>
	        public bool IsCaseSensitive 	{ get; set; }


            /// <summary>
            /// The conversion mode from "direct", "regex", "handler", "list"
            /// </summary>
	        public int ConvertMode 		{ get; set; }


            /// <summary>
            /// The regex pattern to use for conversion checking.
            /// </summary>
	        public string RegexPattern 		{ get; set; }


            /// <summary>
            /// The list of allowed source values
            /// </summary>
	        public List<string> AllowedVals { get; set; }


            /// <summary>
            /// A method handler for more complex conversions.
            /// </summary>
	        public Func<ConvertSpec, object, object> Handler { get; set;}


            /// <summary>
            /// Lookup key
            /// </summary>
            /// <returns></returns>
            public string LookupKey()
            {
                return this.SourceType + "-" + this.DestType;
            }
        }


        private static Dictionary<string, ConvertSpec> _conversionLookup = new Dictionary<string, ConvertSpec>();
        private static List<ConvertSpec> _conversionSpecs = new List<ConvertSpec>()
        {
	        // 				source		dest	   can change		case sensitive	convert mode	regex	handler,  	allowedvalues
	        new ConvertSpec("string",	"string" , true ,			true , 		  	CONVERT_MODE_DIRECT,  	"",  	null, 		                null),
	        new ConvertSpec("string",	"bool"   , true ,			false, 			CONVERT_MODE_LAMBDA, 	"",  	Convert_String_To_Bool,     null),
	        new ConvertSpec("string",	"number" , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	Convert_String_To_Number,   null),
	        new ConvertSpec("string",	"date"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	Convert_String_To_Date,     null),
	        new ConvertSpec("string",	"time"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	Convert_String_To_Time,     null),
	        
            new ConvertSpec("bool"  ,	"string" , true ,			false, 			CONVERT_MODE_TOSTRING,  "",  	null, 		                null),
	        new ConvertSpec("bool"  ,	"bool"   , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                null),
	        new ConvertSpec("bool"  ,	"number" , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	Convert_Bool_To_Number,     null),
	        new ConvertSpec("bool"  ,	"date"   , false,			false, 			CONVERT_MODE_NA,     	"",  	null, 		                null),
	        new ConvertSpec("bool"  ,	"time"   , false,			false, 			CONVERT_MODE_NA,     	"",  	null, 		                null),
	        
            new ConvertSpec("number",	"string" , true ,			false, 			CONVERT_MODE_TOSTRING,  "",  	null, 		                null),
	        new ConvertSpec("number",	"bool"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	Convert_Number_To_Bool,     null),
	        new ConvertSpec("number",	"number" , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                null),
	        new ConvertSpec("number",	"date"   , false,			false, 			CONVERT_MODE_NA,    	"",  	null, /*Convert_Number_To_Date*/ 	null),
	        new ConvertSpec("number",	"time"   , false,			false, 			CONVERT_MODE_NA,    	"",  	null, /*Convert_Number_To_Time*/    null),
	        
            new ConvertSpec("date"  ,	"string" , true ,			false, 			CONVERT_MODE_LAMBDA,    "",  	Convert_Date_To_String,     null),
	        new ConvertSpec("date"  ,	"bool"   , false,			false, 			CONVERT_MODE_NA,     	"",  	null, 		                null),
	        new ConvertSpec("date"  ,	"number" , false ,			false, 			CONVERT_MODE_NA,  	    "",  	null, /*Convert_Date_To_Number*/    null),
	        new ConvertSpec("date"  ,	"date"   , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                null),
	        new ConvertSpec("date"  ,	"time"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	Convert_Date_To_Time,       null),
	        
            new ConvertSpec("time"  ,	"string" , true ,			false, 			CONVERT_MODE_LAMBDA,    "",  	Convert_Time_To_String,     null),
	        new ConvertSpec("time"  ,	"bool"   , false,			false, 			CONVERT_MODE_NA,  	    "",  	null, 		                null),
	        new ConvertSpec("time"  ,	"number" , false ,			false, 			CONVERT_MODE_NA,  	    "",  	null, /*Convert_Time_To_Number*/    null),
	        new ConvertSpec("time"  ,	"date"   , true ,			false, 			CONVERT_MODE_LAMBDA,  	"",  	Convert_Time_To_Date,       null),
	        new ConvertSpec("time"  ,	"time"   , true ,			false, 			CONVERT_MODE_DIRECT,  	"",  	null, 		                null),
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
        public override object Evaluate()
        {
            if (_isConversion)
                return ConvertValue();
            else if (_isConversionCheck)
            {
                var result = TryConvertValue();
                return result;
            }
            return CheckExplicitType();
        }


        /// <summary>
        /// Used for function calls like "is_number(123)"
        /// </summary>
        /// <returns></returns>
        private object CheckExplicitType()
        {
            var val = _exp.Evaluate();
            if (val == null)
                return LNull.Instance;

            var result = false;
            if (     _destinationType == "string" ) result = val.GetType() == typeof(string)  ;
            else if (_destinationType == "number" ) result = val.GetType() == typeof(double)  ;
            else if (_destinationType == "bool"   ) result = val.GetType() == typeof(bool)    ;
            else if (_destinationType == "date"   ) result = val.GetType() == typeof(DateTime);
            else if (_destinationType == "time"   ) result = val.GetType() == typeof(TimeSpan);
            else if (_destinationType == "list"   ) result = val.GetType() == typeof(LArray)  ;
            else if (_destinationType == "map"    ) result = val.GetType() == typeof(LMap)    ;
            return result;
        }

        /// <summary>
        /// Used for function calls like "to_number( '123' )";
        /// </summary>
        /// <returns></returns>
        private object TryConvertValue()
        {
            var val = _exp.Evaluate();
            if (val == null)
                return false;
            var canConvert = false;
            try
            {
                var result = DoConvertValue(_destinationType, val, false);
                if(result != LNull.Instance)
                    canConvert = true;
            }
            catch (Exception)
            {
            }
            return canConvert;
        }


        /// <summary>
        /// Used for function calls like "to_number( '123' )";
        /// </summary>
        /// <returns></returns>
        private object ConvertValue()
        {
            var val = _exp.Evaluate();
            if (val == null)
                return LNull.Instance;
            var result = DoConvertValue(_destinationType, val, true);
            return result;
        }


        private object DoConvertValue(string destinationType, object val, bool handleError)
        {
            // get the source type
            var sourceType = GetTypeName(val);
            var key = sourceType + "-" + destinationType;

            // 1. Check if conversion even exists
            if (!_conversionLookup.ContainsKey(key)) return LNull.Instance;

            // 2. Get the conversion and check it source can be converted to dest.
            var spec = _conversionLookup[key];
            if (!spec.CanChange) return LNull.Instance;

            // 3a. See if there can be a direct conversion.
            if (spec.ConvertMode == CONVERT_MODE_DIRECT)
                return val;

            // 3b. ToString
            if (spec.ConvertMode == CONVERT_MODE_TOSTRING)
                return val.ToString();

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
            if( val.GetType() == typeof(string)   ) return "string";
            if( val.GetType() == typeof(double)   ) return "number";
            if( val.GetType() == typeof(bool)     ) return "bool";
            if( val.GetType() == typeof(DateTime) ) return "date";
            if( val.GetType() == typeof(TimeSpan) ) return "time";
            if( val.GetType() == typeof(LArray)   ) return "list";
            if( val.GetType() == typeof(LMap)     ) return "map";
            return "unknown";
        }


        private static object Convert_Bool_To_Number  (ConvertSpec spec, object val) { return ((bool)val) == true ? 1 : 0; }
        private static object Convert_Number_To_Bool  (ConvertSpec spec, object val) { return ((double)val) > 0 ? true : false; }
        private static object Convert_Date_To_String  (ConvertSpec spec, object val) { return ((DateTime)val).ToString("MM/DD/yyyy hh:mm tt"); }
        private static object Convert_Date_To_Time    (ConvertSpec spec, object val) { return ((DateTime)val).TimeOfDay; }
        private static object Convert_Time_To_String  (ConvertSpec spec, object val) { return ((TimeSpan)val).ToString("hh:mm tt"); }
        //private static object Convert_Number_To_Date(ConvertSpec spec, object val) { return new DateTime(Convert.ToInt64(val)); }
        //private static object Convert_Number_To_Time(ConvertSpec spec, object val) { return new TimeSpan(Convert.ToInt64(val)); }
        //private static object Convert_Date_To_Number(ConvertSpec spec, object val) { return Convert.ToDouble(((DateTime)val).Ticks); }
        //private static object Convert_Time_To_Number(ConvertSpec spec, object val) { return Convert.ToDouble(((TimeSpan)val).Ticks); }
        private static object Convert_Time_To_Date    (ConvertSpec spec, object val) 
        {
            var t = (TimeSpan)val;
            var d = DateTime.Today;            
            return new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds);
        }
        private static object Convert_String_To_Number(ConvertSpec spec, object val) { return Convert.ChangeType(val, typeof(double), null); }
        private static object Convert_String_To_Date(ConvertSpec spec, object val) { return Convert.ChangeType(val, typeof(DateTime), null); }
        private static object Convert_String_To_Time(ConvertSpec spec, object val) 
        {
            string txt = ((string)val).ToLower();
            var result = TimeTypeHelper.ParseTime(txt);
            if (!result.Item2)
                return LNull.Instance;
            return result.Item1;
        }
        private static object Convert_String_To_Bool(ConvertSpec spec, object val)
        {
            var s = ((string)val).ToLower();
            if (s == "yes" || s == "true" || s == "1" || s == "ok" || s == "on")
                return true;
            return false;
        }        
    }
}
