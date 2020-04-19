using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Axion.Core.Source;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Span of source code / Tree leaf with parent and children nodes.
    /// </summary>
    public class Node {
        [JsonIgnore]
        public Unit Source { get; set; }

        public Location Start { get; private set; }
        public Location End   { get; private set; }

        private Ast ast = null!;

        internal Ast Ast {
            get {
                if (ast != null) {
                    return ast;
                }

                Node p = this;
                while (!(p is Ast)) {
                    p = p.Parent;
                }

                ast = (Ast) p;
                return ast;
            }
        }

        internal ITreePath Path = null!;

        [NoPathTraversing]
        protected internal Node? Parent { get; set; }

        private TypeName valueType = null!;

        [JsonIgnore]
        [NoPathTraversing]
        public virtual TypeName ValueType {
            get => valueType;
            protected internal set => valueType = Bind(value);
        }

        public Node(Unit source, Location start = default, Location end = default) {
            Source = source;
            Start  = start;
            End    = end;
        }

        internal T? GetParent<T>()
            where T : Node {
            Node p = this;
            if (p is Ast) {
                if (typeof(T).IsSubclassOf(typeof(ScopeExpr))) {
                    return (T) p;
                }

                return null;
            }

            while (true) {
                p = p.Parent;
                if (p == null || p is T) {
                    return (T) p;
                }

                if (p is Ast) {
                    return null;
                }
            }
        }

        protected static Node GetParentFromChildren(params Node?[] nodes) {
            foreach (Node? n in nodes) {
                if (n?.Parent != null) {
                    return n.Parent;
                }
            }
            throw new ArgumentNullException(
                nameof(nodes),
                "Cannot create instance of expression: unable to get it's parent neither from argument, nor from child expressions."
            );
        }

        protected T Bind<T>(T? value, [CallerMemberName] string callerName = "")
            where T : Node {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            value.Parent = this;
            value.Path   = new NodeTreePath(value, GetType().GetProperty(callerName));

            return value;
        }

        protected T? BindNullable<T>(T? value, [CallerMemberName] string callerName = "")
            where T : Node {
            if (value == null) {
                return value;
            }
            value.Parent = this;
            value.Path   = new NodeTreePath(value, GetType().GetProperty(callerName));

            return value;
        }

        protected NodeList<T> Bind<T>([NotNull] NodeList<T> value)
            where T : Node {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Count == 0) {
                return new NodeList<T>(this);
            }
            for (var i = 0; i < value.Count; i++) {
                if (value[i] is Node n) {
                    n.Parent = this;
                    n.Path   = new NodeListTreePath<T>(value, i);
                }
            }
            return value;
        }

        protected NodeList<T> BindNullable<T>(NodeList<T>? value)
            where T : Node {
            if (value == null || value.Count == 0) {
                return new NodeList<T>(this);
            }
            for (var i = 0; i < value.Count; i++) {
                if (value[i] is Node n) {
                    n.Parent = this;
                    n.Path   = new NodeListTreePath<T>(value, i);
                }
            }
            return value;
        }

        #region Location marking methods

        internal void MarkStart(Location start) {
            Start = start;
        }

        internal void MarkEnd(Location end) {
            End = end;
        }

        internal void MarkStart(Node? mark) {
            Start = mark?.Start ?? Start;
        }

        internal void MarkEnd(Node? mark) {
            End = mark?.End ?? End;
        }

        internal void MarkPosition(Node mark) {
            Start = mark.Start;
            End   = mark.End;
        }

        internal void MarkPosition(Node start, Node end) {
            Start = start.Start;
            End   = end.End;
        }

        #endregion

        public override string ToString() {
            return "from " + Start + " to " + End;
        }
    }
}
