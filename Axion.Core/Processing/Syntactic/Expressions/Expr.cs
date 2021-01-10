using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Axion.Core.Processing.Translation;

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
    [DebuggerDisplay("{" + nameof(debuggerDisplay) + ",nq}")]
    public class Expr : Node {
        internal TokenStream Stream => Unit.TokenStream;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay {
            get {
                var cw = CodeWriter.Default;
                cw.Write(this);
                return cw.ToString();
            }
        }

        protected Expr(Node? parent) : base(
            parent?.Unit!,
            parent?.Start ?? default,
            parent?.End   ?? default
        ) {
            Parent = parent!;
        }

        protected ScopeExpr InitIfNull(
            ref                ScopeExpr? n,
            [CallerMemberName] string     propertyName = ""
        ) {
            n ??= new ScopeExpr(this);
            n =   Bind(n, propertyName);
            return n;
        }
    }
}
