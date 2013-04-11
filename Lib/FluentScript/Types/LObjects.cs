

namespace Fluentscript.Lib.Types
{
    /// <summary>
    /// Holder for "singleton" like object such as empty string, null.
    /// </summary>
    public class LObjects
    {
        /// <summary>
        /// Empty object
        /// </summary>
        public static LNull Null = new LNull(null);


        /// <summary>
        /// Empty string.
        /// </summary>
        public static LObject EmptyString = new LString("");
    }
}
