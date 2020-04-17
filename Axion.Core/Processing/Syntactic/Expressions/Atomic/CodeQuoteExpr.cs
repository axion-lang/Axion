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
        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        [NoPathTraversing]
        public override TypeName ValueType => Scope.ValueType;

        public CodeQuoteExpr(Expr parent) : base(parent) { }

        public CodeQuoteExpr Parse() {
            Scope ??= new ScopeExpr(this);
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
    }
}