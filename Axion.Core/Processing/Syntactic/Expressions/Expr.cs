using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Source;

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
    public class Expr : Node {
        internal TokenStream Stream => Source.TokenStream;
        protected Expr() : base(null) { }

        internal Expr(Node parent) : base(parent.Source, parent.Start, parent.End) {
            Parent = parent;
        }

        protected NodeList<T> InitIfNull<T>(ref NodeList<T> list)
            where T : Node {
            if (list == null) {
                list = new NodeList<T>(this);
            }
            return list;
        }

        protected void SetSpan(Action constructor) {
            Start = Stream.Peek.Start;
            constructor();
            End = Stream.Token.End;
        }

        public override string ToString() {
            var cw = new CodeWriter(new ProcessingOptions("axion", false));
            cw.Write(this);
            return cw.ToString();
        }
    }
}
