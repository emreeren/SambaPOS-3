using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Used to represent a version number e.g. 0.9.8.7
    /// </summary>
    public class LVersion
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="v"></param>
        public LVersion(Version v)
        {
            this.Major = v.Major;
            this.Minor = v.Minor;
            this.Build = v.Build;
            this.Revision = v.Revision;
        }


        /// <summary>
        /// The First unit of the version.
        /// </summary>
        public int Major { get; set; }


        /// <summary>
        /// The second unit of the version.
        /// </summary>
        public int Minor { get; set; }


        /// <summary>
        /// The third unit of the version.
        /// </summary>
        public int Build { get; set; }


        /// <summary>
        /// The fourth unit of the version.
        /// </summary>
        public int Revision { get; set; }
        

        /// <summary>
        /// text based representation of the version.
        /// </summary>
        /// <returns></returns>
        public string Text()
        {
            var text = this.Major + "." + this.Minor + "." + this.Build;
            if (this.Revision < 0)
                return text;

            return text + "." + this.Revision.ToString();
        }
    }
}
