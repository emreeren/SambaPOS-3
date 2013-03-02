using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang.AST
{
    /// <summary>
    /// Marker class for constant or variables expressions that are indexable.
    /// e.g. A declared array or linq expression will be indexable.
    /// </summary>
    public class IndexableExpr : Expr
    {
        /// <summary>
        /// Whether or not this is of the node type supplied.
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public override bool IsNodeType(string nodeType)
        {
            if (nodeType == NodeTypes.SysIndexable)
                return true;
            return base.IsNodeType(nodeType);
        }
    }
}
