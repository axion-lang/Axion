using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Axion.Core.Source;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
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
    ///     </c>
    /// </summary>
    public class Expr : Span {
        private Ast ast = null!;

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

        // TODO: extract to Node class
        internal T? GetParent<T>() where T : Expr {
            Expr p = this;
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

        internal ITreePath Path = null!;

        [NoPathTraversing]
        protected internal Expr? Parent { get; set; }

        private TypeName valueType = null!;

        [JsonIgnore]
        [NoPathTraversing]
        public virtual TypeName ValueType {
            get => valueType;
            protected internal set => valueType = Bind(value);
        }

        internal Type?        MacroExpectType;
        internal TokenStream Stream => Source.TokenStream;
        protected Expr() : base(null) { }

        internal Expr(Expr parent) : base(parent.Source, parent.Start, parent.End) {
            Parent = parent;
        }

        protected static Expr GetParentFromChildren(params Expr?[] initExpressions) {
            foreach (Expr? expr in initExpressions) {
                if (expr?.Parent != null) {
                    return expr.Parent;
                }
            }
            throw new ArgumentNullException(
                nameof(initExpressions),
                "Cannot create instance of expression: unable to get it's parent neither from argument, nor from child expressions."
            );
        }

        protected T Bind<T>(
            T?                        value,
            [CallerMemberName] string callerName = ""
        ) where T : Expr {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            value.Parent = this;
            value.Path   = new NodeTreePath(value, GetType().GetProperty(callerName));

            return value;
        }

        protected T? BindNullable<T>(
            T?                        value,
            [CallerMemberName] string callerName = ""
        ) where T : Expr {
            if (value == null) {
                return value;
            }
            value.Parent = this;
            value.Path   = new NodeTreePath(value, GetType().GetProperty(callerName));

            return value;
        }

        protected NodeList<T> Bind<T>([NotNull] NodeList<T> value)
            where T : Expr {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Count == 0) {
                return new NodeList<T>(this);
            }
            for (var i = 0; i < value.Count; i++) {
                if (value[i] is Expr expr) {
                    expr.Parent = this;
                    expr.Path   = new NodeListTreePath<T>(value, i);
                }
            }
            return value;
        }

        protected NodeList<T> BindNullable<T>(NodeList<T>? value)
            where T : Expr {
            if (value == null || value.Count == 0) {
                return new NodeList<T>(this);
            }
            for (var i = 0; i < value.Count; i++) {
                if (value[i] is Expr expr) {
                    expr.Parent = this;
                    expr.Path   = new NodeListTreePath<T>(value, i);
                }
            }
            return value;
        }

        protected void SetSpan(Action constructor) {
            MarkStart(Stream.Peek);
            constructor();
            MarkEnd(Stream.Token);
        }

        public override string ToString() {
            var cw = new CodeWriter(ProcessingOptions.ToAxion);
            cw.Write(this);
            return cw.ToString();
        }
    }
}