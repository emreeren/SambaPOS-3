
using System.Collections.Generic;


namespace ComLib.Lang.Parsing.MetaPlugins
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
    }
}
