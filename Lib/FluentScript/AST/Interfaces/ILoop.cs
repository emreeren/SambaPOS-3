using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Interface for a loop
    /// </summary>
    public interface ILoop
    {
        /// <summary>
        /// Continue to next iteration.
        /// </summary>
        bool DoContinueLoop { get; set; }


        /// <summary>
        /// Break the loop.
        /// </summary>
        bool DoBreakLoop { get; set; }


        bool DoContinueRunning { get; set; }
    }
}
