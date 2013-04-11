
using System.Collections.Generic;

namespace Fluentscript.Lib.Parser.MetaPlugins
{
    // Used to represent a token match for the grammer check in a plugin.
    public class TokenGroup : TokenMatch
    {
        public List<TokenMatch> Matches;


        public TokenGroup()
        {
            this.Matches = new List<TokenMatch>();
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

            if (this.Matches == null || this.Matches.Count == 0)
                return 0;

            var totalReq = 0;
            for (var ndx = 0; ndx < this.Matches.Count; ndx++)
            {
                var match = this.Matches[ndx];                
                totalReq += match.TotalRequired();
                
            }
            return totalReq;
        }
    }
}
