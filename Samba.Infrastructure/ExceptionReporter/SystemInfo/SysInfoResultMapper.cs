using System.Collections.Generic;
using System.Text;

namespace Samba.Infrastructure.ExceptionReporter.SystemInfo
{
    ///<summary>
    /// Map SysInfoResults to human-readable formats
    ///</summary>
    public static class SysInfoResultMapper
    {

        /// <summary>
        /// create a string representation of a list of SysInfoResults, customised specifically (eg 2-level deep)
        /// </summary>
        public static string CreateStringList(IEnumerable<SysInfoResult> results)
        {
            var stringBuilder = new StringBuilder();

            foreach (var result in results)
            {
                stringBuilder.AppendLine(result.Name);

                foreach (var nodeValueParent in result.Nodes)
                {
                    stringBuilder.AppendLine("-" + nodeValueParent);

                    foreach (var childResult in result.ChildResults)
                    {
                        foreach (var nodeValue in childResult.Nodes)
                        {
                            stringBuilder.AppendLine("--" + nodeValue);		
                        }
                    }
                }
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
    }
}