namespace Fluentscript.Lib.AST.Interfaces
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
