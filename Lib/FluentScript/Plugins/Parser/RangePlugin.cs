using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Helpers;
using ComLib.Lang.Types;
using ComLib.Lang.Parsing;
// </lang:using>

namespace ComLib.Lang.Plugins
{

    /* *************************************************************************
    <doc:example> 
    // Range plugin is used to represent a range e.g. 1..10
    </doc:example>
    ***************************************************************************/
    /// <summary>
    /// Combinator for handling comparisons.
    /// </summary>
    public class RangePlugin : ExprPlugin
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public RangePlugin()
        {
            this.StartTokens = new string[] { "$NumberToken" };
        }


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            var n = _tokenIt.Peek(1).Token;
            if (n != Tokens.Dot) return false;
            n = _tokenIt.Peek(2).Token;
            if (n != Tokens.Dot) return false;
            return true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "<number> .. <number>"; }
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
                    "1..10"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            // 1. min
            var min = Convert.ToDouble(_tokenIt.NextToken.Token);

            // 2. "." "."
            _tokenIt.Advance(2);

            // 3. max
            var max = _tokenIt.ExpectNumber();

            return new ConstantExpr(new LRange(min, max));
        }
    }


    public class LRange : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LRange(double min, double max)
        {
            this.Min = min;
            this.Max = max;
            this.Type = new LRangeType();
        }


        /// <summary>
        /// Min
        /// </summary>
        public double Min { get; set; }


        /// <summary>
        /// Max
        /// </summary>
        public double Max { get; set; }


        /// <summary>
        /// Gets the value of this object.
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return null;
        }


        /// <summary>
        /// Clones this value.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new LRange(this.Min, this.Max);
        }
    }



    public class LRangeType : LObjectType
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        public LRangeType()
        {
            this.Name = "range";
            this.FullName = "sys.range";
            this.TypeVal = 40;
        }


        /// <summary>
        /// Sets up the matrix of possible conversions from one type to another type.
        /// </summary>
        public override void SetupConversionMatrix()
        {
            this.SetDefaultConversionMatrix(TypeConversionMode.NotSupported);
        }
    }
}