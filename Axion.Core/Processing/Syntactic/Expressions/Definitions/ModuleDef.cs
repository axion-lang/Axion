using Axion.Core.Processing.CodeGen;
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
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        private ScopeExpr scope;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        internal ModuleDef(
            string?    name  = null,
            ScopeExpr? scope = null
        ) : this(null, new NameExpr(name), scope) { }

        internal ModuleDef(
            Expr?      parent = null,
            NameExpr?  name   = null,
            ScopeExpr? scope  = null
        ) : base(
            parent
         ?? GetParentFromChildren(name, scope)
        ) {
            Name  = name;
            Scope = scope;
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

        public override void ToAxion(CodeWriter c) {
            c.Write("module ", Name, Scope);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("namespace ", Name);
            c.WriteLine();
            c.Write(Scope);
        }

        public override void ToPython(CodeWriter c) {
            c.AddJoin("", Scope.Items, true);
        }
    }
}