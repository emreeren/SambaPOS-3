
namespace Fluentscript.Lib.Parser.MetaPlugins
{
    // Used to represent a token match for the grammer check in a plugin.
    public class TokenGroupOr : TokenMatch
    {
        public TokenMatch Left;
        public TokenMatch Right;


        public TokenGroupOr(TokenMatch left, TokenMatch right)
        {
            this.Left = left;
            this.Right = right;
            this.IsGroup = true;
        }


        /// <summary>
        /// Gets the total number of required plugins.
        /// </summary>
        /// <returns></returns>
        public override int TotalRequired()
        {
            if (!this.IsRequired) 
                return 0;

            if (this.Left == null && this.Right == null)
                return 0;

            var totalReq = this.Left.TotalRequired() + this.Right.TotalRequired();
            return totalReq;
        }
    }
}
