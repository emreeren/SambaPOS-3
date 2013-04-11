using System.Collections.Generic;
using Fluentscript.Lib.AST;

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Abstract syntax tree node.
    /// </summary>
    public class AstNode
    {
        private List<AstNode> _children;


        /// <summary>
        /// Reference to the script.
        /// </summary>
        public ScriptRef Ref { get; set; }


        /// <summary>
        /// The referencing token.
        /// </summary>
        public TokenData Token { get; set; }

        
        /// <summary>
        /// The node type.
        /// </summary>
        public string Nodetype;


        /// <summary>
        /// Number of children in this node.
        /// </summary>
        /// <returns></returns>
        public int ChildCount()
        {
            if (_children == null) return 0;
            return _children.Count;
        }


        /// <summary>
        /// Adds a child to this node.
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(AstNode node)
        {
            if (_children == null)
                _children = new List<AstNode>();
            _children.Add(node);
        }


        /// <summary>
        /// Get a child at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AstNode GetChild(int index)
        {
            if (_children == null || _children.Count == 0) return null;
            return _children[index];
        }


        /// <summary>
        /// Initialize the boundary information.
        /// </summary>
        /// <param name="supportsBoundary"></param>
        /// <param name="boundaryText"></param>
        public virtual void InitBoundary(bool supportsBoundary, string boundaryText)
        {
            //_supportsBoundary = supportsBoundary;
            //_boundaryText = boundaryText;
        }


        /// <summary>
        /// Whether or not this is the same type as the nodeType supplied.
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public virtual bool IsNodeType(string nodeType)
        {
            return this.Nodetype == nodeType;
        }


        /// <summary>
        /// Returns the fully qualified name of this node.
        /// </summary>
        /// <returns></returns>
        public virtual string ToQualifiedName()
        {
            return string.Empty;
        }


        /// <summary>
        /// Parent of this statement
        /// </summary>
        public AstNode Parent { get; set; }


        /// <summary>
        /// Finds the first parent that is of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T FindParent<T>() where T : class
        {
            T found = default(T);
            var current = Parent;
            while (current != null)
            {
                if (current is T)
                {
                    found = current as T;
                    break;
                }
                current = current.Parent;
            }
            return found;
        }
    }
}
