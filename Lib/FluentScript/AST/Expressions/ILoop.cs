using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Interface for a loop
    /// </summary>
    internal interface ILoop
    {
        /// <summary>
        /// Continue to next iteration.
        /// </summary>
        void Continue();


        /// <summary>
        /// Break the loop.
        /// </summary>
        void Break();
    }
}
