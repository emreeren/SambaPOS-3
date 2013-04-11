using System.Collections.Generic;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.Integration
{
    /// <summary>
    /// Helper class for calling functions
    /// </summary>
    public class RegisteredDirectives
    {
        private IDictionary<string, bool> _directives;
        private List<string> _directiveStack;
 

        /// <summary>
        /// Initialize
        /// </summary>
        public RegisteredDirectives()
        {
            this._directives = new Dictionary<string, bool>();
            this._directiveStack = new List<string>();
        }


        /// <summary>
        /// Register a directive
        /// </summary>
        /// <param name="directive">The directive to register</param>
        public void Register(string directive)
        {
            this._directives[directive] = true;
        }


        public void RegisterDelimited(string directivesDelimited)
        {
            if (string.IsNullOrEmpty(directivesDelimited))
                return;

            var dirs = directivesDelimited.Split(',');
            if (dirs.Length > 0)
            {
                foreach (var dir in dirs)
                {
                    this.Register(dir);
                }
            }
        }


        /// <summary>
        /// Whether or not the directive is present.
        /// </summary>
        /// <param name="directive"></param>
        /// <returns></returns>
        public bool Contains(string directive)
        {
            return this._directives.ContainsKey(directive);
        }


        /// <summary>
        /// The total items on the directive stack
        /// </summary>
        /// <returns></returns>
        public int StackCount()
        {
            return _directiveStack.Count;
        }


        /// <summary>
        /// Call to keep track of current directive in code.
        /// </summary>
        /// <param name="word"></param>
        public void StartDirectiveCode(string word)
        {
            _directiveStack.Add(word);
        }


        /// <summary>
        /// Called on end of the directive code block.
        /// </summary>
        public void EndDirectiveCode()
        {
            _directiveStack.RemoveAt(_directiveStack.Count-1);
        }
    }
}
