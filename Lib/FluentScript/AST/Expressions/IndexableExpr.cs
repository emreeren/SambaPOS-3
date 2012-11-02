using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;


namespace ComLib.Lang
{
    /// <summary>
    /// Marker class for constant or variables expressions that are indexable.
    /// e.g. A declared array or linq expression will be indexable.
    /// </summary>
    public class IndexableExpr : Expr
    {
    }
}
