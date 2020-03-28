using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         code-quote-expr:
    ///             '{{' expr '}}';
    ///     </c>
    /// </summary>
    public class CodeQuoteExpr : AtomExpr {
        private ScopeExpr scope;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Scope.ValueType;

        public CodeQuoteExpr(
            Expr?      parent = null,
            ScopeExpr? scope  = null
        ) : base(
            parent
         ?? GetParentFromChildren(scope)
        ) {
            Scope = scope ?? new ScopeExpr(this);
        }

        public CodeQuoteExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(DoubleOpenBrace);
                    while (!Stream.PeekIs(DoubleCloseBrace, TokenType.End)) {
                        Scope.Items.Add(AnyExpr.Parse(this));
                    }

                    Stream.Eat(DoubleCloseBrace);
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("{{", Scope, "}}");
        }
    }
}