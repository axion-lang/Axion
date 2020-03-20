using System.Reflection;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.Traversal {
    /// <summary>
    ///     Data structure designed to simplify AST traversing.
    ///     It binds each child to parent by reference,
    ///     so you can dynamically modify/replace child expressions.
    /// </summary>
    public interface ITreePath {
        bool Traversed { get; set; }
        Expr Node      { get; set; }
    }

    public class NodeTreePath : ITreePath {
        private readonly PropertyInfo refToNodeInParent;
        private readonly Expr         node;
        public           bool         Traversed { get; set; } = false;

        public Expr Node {
            get => node;
            set => refToNodeInParent.SetValue(node.Parent, value);
        }

        public NodeTreePath(Expr node, PropertyInfo refToNodeInParent) {
            this.node              = node;
            this.refToNodeInParent = refToNodeInParent;
        }
    }

    public class NodeListTreePath<T> : ITreePath where T : Expr {
        private readonly NodeList<T> list;
        internal         int         IndexInList;
        public           bool        Traversed { get; set; } = false;

        public Expr Node {
            get => list[IndexInList];
            set => list[IndexInList] = (T) value;
        }

        public NodeListTreePath(NodeList<T> list, int indexInList) {
            this.list   = list;
            IndexInList = indexInList;
        }
    }
}