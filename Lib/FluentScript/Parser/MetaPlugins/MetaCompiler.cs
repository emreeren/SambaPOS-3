using System;
using ComLib.Lang.AST;
using ComLib.Lang.Core;
using ComLib.Lang.Types;

namespace ComLib.Lang.Parsing.MetaPlugins
{
    public class MetaCompiler
    {
        private MetaCompilerData _data;


        /// <summary>
        /// Metacompiler.
        /// </summary>
        public MetaCompiler()
        {
            _data = new MetaCompilerData();    
            _data.Init();
        }


        public Context Ctx;


        /// <summary>
        /// Builds a literal date expression from the inputs.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public Token ToConstDateTimeToken(TokenData date, TokenData time)
        {
            var d = (DateTime)date.Token.Value;
            var t = (TimeSpan) time.Token.Value;
            var datetime = new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds);
            var text = date.Token.Text + " " + time.Token.Text;
            var token = date.Token.Clone();
            token.SetTextAndValue(text, datetime);
            return token;
        }


        /// <summary>
        /// Builds a literal date expression from the inputs.
        /// </summary>
        /// <param name="month">The month of the year</param>
        /// <param name="day">The day of the month from 1-31</param>
        /// <param name="year">The year</param>
        /// <param name="token">The token representing the start of the date e.g. month.</param>
        /// <returns></returns>
        public Expr ToConstDate(int month, int day, int year, TokenData token)
        {
            var date = new DateTime(month, day, year);
            return Exprs.Const(new LDate(date), token);
        }


        /// <summary>
        /// Creates a constant time expression out of the inputs supplied.
        /// </summary>
        /// <param name="hours">The hours</param>
        /// <param name="minutes">The minutes</param>
        /// <param name="seconds">The seconds</param>
        /// <param name="token">The token representing the time</param>
        /// <returns></returns>
        public Expr ToConstTime(int hours, int minutes, int seconds, TokenData token)
        {
            var time = new TimeSpan(0, hours, minutes, seconds);
            return Exprs.Const(new LTime(time), token);
        }


        /// <summary>
        /// Creates a constant time expression out of the inputs supplied.
        /// </summary>
        /// <param name="day">The number representation for a day of the week</param>
        /// <param name="token">The token associated with the number</param>
        /// <returns></returns>
        public Expr ToConstDay(int day, TokenData token)
        {
            var d = this._data.LookupDay(day);
            return Exprs.Const(new LDayOfWeek(d), token);
        }
    }
}
