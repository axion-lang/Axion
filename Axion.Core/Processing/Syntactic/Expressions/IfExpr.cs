using Axion.Core.Processing.CodeGen;
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
    public class IfExpr : Expr, IDecorableExpr {
        private Expr condition;

        public Expr Condition {
            get => condition;
            set => condition = Bind(value);
        }

        private ScopeExpr thenScope;

        public ScopeExpr ThenScope {
            get => thenScope;
            set => thenScope = Bind(value);
        }

        private ScopeExpr elseScope;

        public ScopeExpr ElseScope {
            get => elseScope;
            set => elseScope = Bind(value);
        }

        internal IfExpr(
            Expr?      parent    = null,
            Expr?      condition = null,
            ScopeExpr? thenScope = null,
            ScopeExpr? elseScope = null
        ) : base(
            parent
         ?? GetParentFromChildren(condition, thenScope, elseScope)
        ) {
            Condition = condition;
            ThenScope = thenScope;
            ElseScope = elseScope;
        }

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
                        ElseScope = new ScopeExpr(this, new IfExpr(this).Parse(true));
                    }
                }
            );
            return this;
        }

        public override void ToDefault(CodeWriter c) {
            c.Write("if ", Condition, ThenScope);
            if (ElseScope != null) {
                c.Write("else", ElseScope);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("if (", Condition);
            c.WriteLine(")");
            c.Write(ThenScope);
            if (ElseScope != null) {
                c.WriteLine("else");
                c.Write(ElseScope);
            }
        }

        public override void ToPascal(CodeWriter c) {
            c.Write("if ", Condition, " then", ThenScope);
            if (ElseScope != null) {
                c.Write("else", ElseScope);
            }
        }
    }
}