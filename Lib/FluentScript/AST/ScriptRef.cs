namespace Fluentscript.Lib.AST
{
    /// <summary>
    /// References to the script name, line number, char position.
    /// </summary>
    public class ScriptRef
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name"></param>
        /// <param name="line"></param>
        /// <param name="charPos"></param>
        public ScriptRef(string name, int line, int charPos)
        {
            this.Line = line;
            this.CharPos = charPos;
            this.ScriptName = name;
        }


        /// <summary>
        /// Script info.
        /// </summary>
        public readonly string ScriptName;


        /// <summary>
        /// Line number in the script.
        /// </summary>
        public readonly int Line;


        /// <summary>
        /// Char position in the line in the script.
        /// </summary>
        public readonly int CharPos; 
    }
}
