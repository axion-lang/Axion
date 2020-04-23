using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         conditional-expr:
    ///             'if' infix-expr scope
    ///             {'elif' infix-expr scope}
    ///             ['else' scope];
    ///     </c>
    /// </summary>
    public class IfExpr : Expr {
        private Expr condition = null!;

        public Expr Condition {
            get => condition;
            set => condition = Bind(value);
        }

        private ScopeExpr thenScope = null!;

        public ScopeExpr ThenScope {
            get => thenScope;
            set => thenScope = Bind(value);
        }

        private ScopeExpr elseScope = null!;

        public ScopeExpr ElseScope {
            get => elseScope;
            set => elseScope = Bind(value);
        }

        internal IfExpr(Node parent) : base(parent) { }

        public IfExpr Parse(bool elseIf = false) {
            SetSpan(
                () => {
                    if (!elseIf) {
                        Stream.Eat(KeywordIf);
                    }

                    Condition = InfixExpr.Parse(this);
                    ThenScope = new ScopeExpr(this).Parse();

                    if (Stream.MaybeEat(KeywordElse)) {
                        ElseScope = new ScopeExpr(this).Parse();
                    }
                    else if (Stream.MaybeEat(KeywordElif)) {
                        ElseScope = new ScopeExpr(this) {
                            Items = new NodeList<Expr>(this) {
                                new IfExpr(this).Parse(true)
                            }
                        };
                    }
                }
            );
            return this;
        }
    }
}
