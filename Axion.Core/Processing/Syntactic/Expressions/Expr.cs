using System;
using System.Runtime.CompilerServices;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         expr_list:
    ///             expr {',' expr};
    ///         infix_list:
    ///             infix_expr {',' infix_expr};
    ///         simple_name_list:
    ///             simple_name_expr {',' simple_name_expr};
    ///         single_expr:
    ///             conditional_expr | while_expr | for_expr    |
    ///             try_expr         | with_expr  | import_expr |
    ///             decorated;
    ///         decorated:
    ///             module_def | class_def  | enum_def |
    ///             func_def   | small_expr;
    ///         small_expr:
    ///             pass_expr | expr_expr | flow_expr;
    ///         flow_expr:
    ///             break_expr | continue_expr | return_expr |
    ///             raise_expr | yield_expr;
    ///     </c>
    /// </summary>
    public class Expr : Span {
        private Ast ast;

        internal Ast Ast {
            get {
                if (ast != null) {
                    return ast;
                }

                Expr p = this;
                while (!(p is Ast)) {
                    p = p.Parent;
                }

                ast = (Ast) p;
                return ast;
            }
        }

        internal T GetParentOfType<T>() where T : Expr {
            Expr p = this;
            if (p is Ast) {
                if (typeof(T).IsSubclassOf(typeof(BlockExpr))) {
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

        internal ITreePath Path;

        [NoTraversePath]
        protected internal Expr Parent { get; set; }

        private TypeName valueType;

        [JsonIgnore]
        [NoTraversePath]
        public virtual TypeName ValueType {
            get => valueType;
            set => SetNode(ref valueType, value);
        }

        internal Type MacroExpectationType;

        internal TokenStream Stream => Source.TokenStream;

        protected Expr() : base(null) { }

        internal Expr(Expr parent) : base(parent.Source, parent.Start, parent.End) {
            Parent = parent;
        }

        protected void SetNode<T>(
            ref T field,
            T     value,
            [CallerMemberName]
            string callerName = ""
        ) where T : Expr {
            if (field == value) {
                return;
            }

            if (value != null) {
                value.Parent = this;
                value.Path   = new NodeTreePath(value, GetType().GetProperty(callerName));
            }

            field = value;
        }

        protected void SetNode<T>(ref NodeList<T> field, NodeList<T> value)
            where T : Expr {
            if (field == value) {
                return;
            }

            if (value != null && value.Count > 0) {
                for (var i = 0; i < value.Count; i++) {
                    if (value[i] is Expr expr) {
                        expr.Parent = this;
                        expr.Path   = new NodeListTreePath<T>(value, i);
                    }
                }
            }
            else {
                value = new NodeList<T>(this);
            }

            field = value;
        }

        protected void SetSpan(Action constructor) {
            MarkStart(Stream.Peek);
            constructor();
            MarkEnd(Stream.Token);
        }
    }
}