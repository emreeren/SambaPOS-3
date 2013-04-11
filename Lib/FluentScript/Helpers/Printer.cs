using System;

namespace Fluentscript.Lib.Helpers
{
    public class Printer
    {
        /// <summary>
        /// Writes out a header text
        /// </summary>
        /// <param name="text"></param>
        public void WriteHeader(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text.ToUpper());
        }


        /// <summary>
        /// Writest the text supplied on 1 line.
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }


        /// <summary>
        /// Writes out a key/value line.
        /// </summary>
        /// <param name="highlightKey"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void WriteKeyValue(bool highlightKey, string key, bool highlightVal, string val)
        {
            if (highlightKey)
                Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(key);
            Console.ResetColor();
            if (highlightVal)
                Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(val);
            Console.ResetColor();
        }


        /// <summary>
        /// Writes out lines
        /// </summary>
        /// <param name="count"></param>
        public void WriteLines(int count)
        {
            for (int ndx = 0; ndx < count; ndx++)
            {
                Console.WriteLine();
            }
        }
    }
}
