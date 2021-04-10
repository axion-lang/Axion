using System.Reflection;
using Axion.Core.Processing.Syntactic;

namespace Axion.Core.Processing.Traversal {
    /// <summary>
    ///     Data structure designed to simplify AST traversing.
    ///     It binds each child to parent by reference,
    ///     so you can dynamically modify/replace child expressions.
    /// </summary>
    public interface ITreePath {
        bool Traversed { get; set; }
        Node Node      { get; set; }
    }

    public class NodeTreePath : ITreePath {
        readonly PropertyInfo refToNodeInParent;
        readonly Node node;

        public bool Traversed { get; set; }

        public Node Node {
            get => node;
            set => refToNodeInParent.SetValue(node.Parent, value);
        }

        public NodeTreePath(Node node, PropertyInfo refToNodeInParent) {
            this.node              = node;
            this.refToNodeInParent = refToNodeInParent;
        }
    }

    public class NodeListTreePath<T> : ITreePath where T : Node {
        readonly NodeList<T> list;
        internal int IndexInList;

        public bool Traversed { get; set; }

        public Node Node {
            get => list[IndexInList];
            set => list[IndexInList] = (T) value;
        }

        public NodeListTreePath(NodeList<T> list, int indexInList) {
            this.list   = list;
            IndexInList = indexInList;
        }
    }
}
