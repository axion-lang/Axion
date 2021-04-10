using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Translation;
using Axion.Core.Processing.Traversal;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Span of source code / Tree leaf with parent and children nodes.
    ///     <code>
    ///         multiple-expr:
    ///             expr {',' expr};
    ///         multiple-infix:
    ///             infix-expr {',' infix-expr};
    ///         simple-multiple-name:
    ///             simple-name-expr {',' simple-name-expr};
    ///         single-expr:
    ///             conditional-expr | while-expr | for-expr    |
    ///             try-expr         | with-expr  | import-expr |
    ///             decorated;
    ///         decorated:
    ///             module-def | class-def  | enum-def |
    ///             func-def   | small-expr;
    ///         small-expr:
    ///             pass-expr | expr-expr | flow-expr;
    ///         flow-expr:
    ///             break-expr | continue-expr | return-expr |
    ///             raise-expr | yield-expr;
    ///     </code>
    /// </summary>
    public abstract class Node : CodeSpan, ITranslatableNode {
        Ast? ast;

        /// <summary>
        ///     Abstract Syntax Tree root of this node.
        ///     <exception cref="NullReferenceException">
        ///         Thrown if node is not completely bound at the moment.
        ///     </exception>
        /// </summary>
        internal Ast Ast {
            get {
                if (ast != null) {
                    return ast;
                }

                var p = this;
                while (!(p is Ast)) {
                    p = p.Parent;
                }

                ast = (Ast) p;
                return ast;
            }
        }

        TypeName? valueType;

        /// <summary>
        ///     Language type-name of this node that can be inferred from context.
        /// </summary>
        [JsonIgnore]
        [NoPathTraversing]
        public virtual TypeName? ValueType {
            get => valueType;
            set => valueType = BindNullable(value);
        }

        /// <summary>
        ///     Direct reference to the attribute of
        ///     parent to which this node is bound.
        /// </summary>
        public ITreePath Path { get; protected set; } = null!;

        /// <summary>
        ///     Reference to parent of this node.
        /// </summary>
        [NoPathTraversing]
        public Node Parent { get; set; } = null!;

        /// <summary>
        ///     Constructor for <see cref="Token"/>s. 
        /// </summary>
        protected Node(
            Unit     unit,
            Location start = default,
            Location end   = default
        ) : base(unit, start, end) { }

        /// <summary>
        ///     Constructor for expressions.
        /// </summary>
        protected Node(Node? parent) : base(
            parent?.Unit!,
            parent?.Start ?? default,
            parent?.End ?? default
        ) {
            Parent = parent!;
        }

        /// <summary>
        ///     Returns first parent of this node with given type.
        ///     (<code>null</code> if parent of given type is not exists).
        /// </summary>
        internal T? GetParent<T>() where T : Node {
            var p = this;
            while (true) {
                p = p.Parent;
                switch (p) {
                case T node: return node;
                case Ast _:  return null;
                }
            }
        }

        // Following are used by source generators.
        // ReSharper disable UnusedMember.Global
        
        protected internal NodeList<T> InitIfNull<T>(ref NodeList<T>? list)
            where T : Node {
            list ??= new NodeList<T>(this);
            list =   Bind(list);
            return list;
        }

        #region Node binding methods

        /// <summary>
        ///     [ONLY-INSIDE-PROPERTY]
        ///     Binds given property value to parent node
        ///     and extends it's span if needed.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if provided property value is null.
        /// </exception>
        protected T Bind<T>(
            T                         value,
            [CallerMemberName] string propertyName = ""
        ) where T : Node {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            return BindNode(value, propertyName);
        }

        /// <summary>
        ///     [ONLY-INSIDE-PROPERTY]
        ///     Binds given property value to parent node
        ///     and extends it's span if needed.
        /// </summary>
        protected internal T? BindNullable<T>(
            T?                        value,
            [CallerMemberName] string propertyName = ""
        ) where T : Node {
            return value == null ? value : BindNode(value, propertyName);
        }

        /// <summary>
        ///     Internal node binding method.
        ///     Creates a path to parent attribute.
        /// </summary>
        T BindNode<T>(T value, string propertyName) where T : Node {
            ExtendSpan(value);

            value.Parent = this;
            value.Path = new NodeTreePath(
                value,
                GetType().GetProperty(propertyName)!
            );

            return value;
        }

        /// <summary>
        ///     Binds child node to parent node
        ///     and extends parent span if needed.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if provided property value is null.
        /// </exception>
        public T Bind<T>(T value, NodeList<T> list, int index) where T : Node {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            ExtendSpan(value);

            value.Parent = this;
            value.Path   = new NodeListTreePath<T>(list, index);

            return value;
        }

        /// <summary>
        ///     [ONLY-INSIDE-PROPERTY]
        ///     Binds given property value to parent node
        ///     and extends it's span if needed.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if provided property value is null.
        /// </exception>
        protected NodeList<T> Bind<T>([NotNull] NodeList<T> list)
            where T : Node {
            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            if (list.Count == 0) {
                return list;
            }

            ExtendSpan(list[0], list[^1]);

            for (var i = 0; i < list.Count; i++) {
                if (list[i] is Node n) {
                    n.Parent = this;
                    n.Path   = new NodeListTreePath<T>(list, i);
                }
            }

            return list;
        }
        
        // ReSharper restore UnusedMember.Global

        #endregion

        #region Location marking methods

        bool firstTimeSpanMarking = true;

        /// <summary>
        ///     Extends this span of code
        ///     if provided mark is out of existing span.
        /// </summary>
        public void ExtendSpan(CodeSpan n) {
            // if span is marked first time, set span equal to starting one
            // to prevent new node spanning from (1,1) to end.
            if (firstTimeSpanMarking) {
                Start                = n.Start;
                End                  = n.End;
                firstTimeSpanMarking = false;
                return;
            }

            if (n.Start < Start) {
                Start = n.Start;
            }

            if (n.End > End) {
                End = n.End;
            }

            // fix negative span
            if (End < Start) {
                End = Start;
            }
        }

        /// <summary>
        ///     Extends this span of code
        ///     if any of provided marks is out of existing span.
        /// </summary>
        public void ExtendSpan(CodeSpan a, CodeSpan b) {
            // if span is marked first time, select least span of a & b.
            // to prevent new node spanning from (1,1) to end.
            if (firstTimeSpanMarking) {
                Start                = Location.Max(a.Start, b.Start);
                End                  = Location.Min(a.End, b.End);
                firstTimeSpanMarking = false;
                return;
            }

            if (a.Start < Start) {
                Start = a.Start;
            }
            else if (b.Start < Start) {
                Start = b.Start;
            }

            if (b.End > End) {
                End = b.End;
            }
            else if (a.End > End) {
                End = a.End;
            }

            // fix negative span
            if (End < Start) {
                End = Start;
            }
        }

        #endregion

        public override string ToString() {
            return "from " + Start + " to " + End;
        }
    }
}
