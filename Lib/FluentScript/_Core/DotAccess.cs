using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang.Core
{
    /// <summary>
    /// Dot access for representing member access fields. e.g. "abc.def.hi"
    /// </summary>
    public class DotAccess
    {
        /// <summary>
        /// Root member of the dot access
        /// </summary>
        public string RootName { get; set; }


        /// <summary>
        /// The root member type
        /// </summary>
        public MemberMode RootType { get; set; }


        /// <summary>
        /// The symbol scope of the first member.
        /// </summary>
        public ISymbols RootScope { get; set; }

                
        /// <summary>
        /// The remaining members
        /// </summary>
        public List<string> Members { get; set; }


        /// <summary>
        /// The full name as a string e.g. "modulea.core.functionname"
        /// </summary>
        public string FullName { get; set; }
    }
}
