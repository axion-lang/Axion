using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         module-def:
    ///             'module' name scope;
    ///     </c>
    /// </summary>
    public class ModuleDef : Expr, IDefinitionExpr, IDecorableExpr {
        private NameExpr name = null!;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        internal ModuleDef(Node parent) : base(parent) { }

        public ModuleDef Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordModule);
                    Name  = new NameExpr(this).Parse();
                    Scope = new ScopeExpr(this).Parse();
                }
            );
            return this;
        }
    }
}
