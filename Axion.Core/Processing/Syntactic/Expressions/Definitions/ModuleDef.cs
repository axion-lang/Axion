using System.Collections.Generic;
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
        private NameExpr? name;

        public NameExpr? Name {
            get => name;
            set => name = BindNullable(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        internal ModuleDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Expr[] items) {
            return new DecoratedExpr(Parent) {
                Target     = this,
                Decorators = new NodeList<Expr>(this, items)
            };
        }

        public ModuleDef WithScope(params Expr[] items) {
            return WithScope((IEnumerable<Expr>) items);
        }

        public ModuleDef WithScope(IEnumerable<Expr> items) {
            Scope = new ScopeExpr(this).WithItems(items);
            return this;
        }

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
