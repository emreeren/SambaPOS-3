using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Runtime.Bindings
{
    /// <summary>
    /// Binding class exposed to the scripting environment to hook into the compiler process.
    /// </summary>
    public class MetaCompiler : LanguageBinding
    {
        private MetaCompilerData _data;


	    public MetaCompiler()
	    {
		    this.Namespace = "sys.compiler";
		    this.SupportsFunctions = true;
	        this.NamingConvention = "pascal";
		    this.ExportedFunctions = new List<string>()
		    {
			    "toConstantDate",
			    "toConstantTime",
			    "toConstantDay",
                "toListCheck",
                "toRelativeDate",
                "toDaysAway",
                "toTable"
		    };
            _data = new MetaCompilerData();
            _data.Init();
	    }


        /// <summary>
        /// The parsing/execution context.
        /// </summary>
        public Context Ctx;

		
	    /// <summary>Converts the parameters to a constant date expression </summary>
	    /// <param name="expr">The function call expressions</param>
	    public object ToConstantDate(BindingCallExpr expr)
	    {
            var month = Convert.ToInt32(expr.ParamList[0]);
            var day = Convert.ToInt32(expr.ParamList[1]);
            var year = Convert.ToInt32(expr.ParamList[2]);
            var time = (string)expr.ParamList[3];
            var token = expr.ParamList[4] as TokenData;
            return Exprs.Date(month, day, year, time, token);
	    }
	
	
	    /// <summary>Converts the parameters to a constant date time token </summary>
	    /// <param name="expr">The function call expressions</param>
        public object ToConstantDateTimeToken(BindingCallExpr expr)
	    {
            var date = expr.ParamList[0] as TokenData;
            var time = expr.ParamList[1] as TokenData;

            var d = (DateTime)date.Token.Value;
            var t = (TimeSpan)time.Token.Value;
            var datetime = new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds);
            var text = date.Token.Text + " " + time.Token.Text;
            var token = date.Token.Clone();
            token.SetTextAndValue(text, datetime);
            return token;
	    }
	
	
	    /// <summary>Converts the parameters to a constant day expression </summary>
	    /// <param name="expr">The function call expressions</param>
        public object ToRelativeDay(BindingCallExpr expr)
	    {
            var day = Convert.ToInt32(expr.ParamList[0]);
            var time = (string)expr.ParamList[1];
            var token = expr.ParamList[2] as TokenData;
            var dayName = _data.LookupDayName(day);
            return Exprs.Day(dayName, time, token);
	    }


        /// <summary>Converts the parameters to a constant day expression </summary>
        /// <param name="expr">The function call expressions</param>
        public object ToRelativeDate(BindingCallExpr expr)
        {
            var relativeDay = (string)(expr.ParamList[0]);
            var dayOfWeek = Convert.ToInt32(expr.ParamList[1]);
            var month = Convert.ToInt32(expr.ParamList[2]);
            var token = expr.ParamList[3] as TokenData;
            return Exprs.DateRelative(relativeDay, dayOfWeek, month, token);
        }


        /// <summary>Converts the parameters to a constant day expression </summary>
        /// <param name="expr">The function call expressions</param>
        public object ToDuration(BindingCallExpr expr)
        {
            var duration = (string)(expr.ParamList[0]);
            var type = (string) expr.ParamList[1];
            var token = expr.ParamList[2] as TokenData;
            return Exprs.Duration(duration, type, token);
        }


        /// <summary>
        /// Creates a constant string
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object ToListCheck(BindingCallExpr expr)
        {
            var name = expr.ParamList[0] as TokenData;
            var token = expr.ParamList[1] as TokenData;
            return Exprs.ListCheck(name, token);
        }


        public object ToEnumerableLoop(BindingCallExpr expr)
        {
            var token = expr.ParamList[2] as TokenData;
            var varname = (string)expr.ParamList[0];
            var enableWith = (bool)expr.ParamList[1];
            var collectionName = varname + "s";
            var varExpr = Exprs.Ident(collectionName, token);
            var loopexpr = Exprs.ForEach(varname, varExpr, token);
            ((BlockExpr)loopexpr).EnableAutoVariable = enableWith;
            return loopexpr;
        }


        public object ToTable(BindingCallExpr expr)
        {
            var fields = expr.ParamList[0] as List<object>;
            var fieldNames = new List<string>();
            foreach(var obj in fields)
            {
                fieldNames.Add(Convert.ToString(obj));
            }
            var start = expr.ParamList[1] as TokenData;
            var exp = Exprs.Table(fieldNames, start);
            return exp;
        }


        /// <summary>
        /// Creates a constant string
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object ToConstantString(BindingCallExpr expr)
        {
            var text = expr.ParamList[0] as TokenData;
            var token = expr.ParamList[1] as TokenData;
            return Exprs.Const(new LString(text.Token.Text), token);
        }


        /// <summary>
        /// Creates a constant number
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object ToConstantNumber(BindingCallExpr expr)
        {
            var num = Convert.ToDouble(expr.ParamList[0]);
            var token = expr.ParamList[1] as TokenData;
            return Exprs.Const(new LNumber(num), token);
        }
    }
}
