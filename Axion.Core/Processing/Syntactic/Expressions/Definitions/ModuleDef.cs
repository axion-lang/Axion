using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         module-def:
    ///             'module' name scope;
    ///     </c>
    /// </summary>
    public class ModuleDef : Node, IDefinitionExpr, IDecorableExpr {
        private Token? kwModule;

        public Token? KwModule {
            get => kwModule;
            set => kwModule = BindNullable(value);
        }

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

        public ModuleDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Node[] items) {
            return new(Parent) {
                Target     = this,
                Decorators = new NodeList<Node>(this, items)
            };
        }

        public ModuleDef WithScope(params Node[] items) {
            return WithScope((IEnumerable<Node>) items);
        }

        public ModuleDef WithScope(IEnumerable<Node> items) {
            Scope = new ScopeExpr(this).WithItems(items);
            return this;
        }

        public ModuleDef Parse() {
            KwModule = Stream.Eat(KeywordModule);
            Name     = new NameExpr(this).Parse();
            Scope    = new ScopeExpr(this).Parse();
            return this;
        }
    }
}
