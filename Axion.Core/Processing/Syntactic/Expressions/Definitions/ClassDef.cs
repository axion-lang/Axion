using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         class-def:
    ///             'class' simple-name [type-args] ['&lt;-' type-multiple-arg] scope;
    ///     </c>
    /// </summary>
    public class ClassDef : Expr, IDefinitionExpr, IDecorableExpr {
        private Token? kwClass;

        public Token? KwClass {
            get => kwClass;
            set => kwClass = BindNullable(value);
        }

        private NameExpr? name;

        public NameExpr? Name {
            get => name;
            set => name = BindNullable(value);
        }

        private NodeList<TypeName>? bases;

        public NodeList<TypeName> Bases {
            get => InitIfNull(ref bases);
            set => bases = Bind(value);
        }

        private NodeList<Expr>? keywords;

        public NodeList<Expr> Keywords {
            get => InitIfNull(ref keywords);
            set => keywords = Bind(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        private NodeList<Expr>? dataMembers;

        public NodeList<Expr> DataMembers {
            get => InitIfNull(ref dataMembers);
            set => dataMembers = Bind(value);
        }

        public ClassDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Expr[] items) {
            return new DecoratedExpr(Parent) {
                Target     = this,
                Decorators = new NodeList<Expr>(this, items)
            };
        }

        public ClassDef WithScope(params Expr[] items) {
            return WithScope((IEnumerable<Expr>) items);
        }

        public ClassDef WithScope(IEnumerable<Expr> items) {
            Scope = new ScopeExpr(this).WithItems(items);
            return this;
        }

        public ClassDef Parse() {
            KwClass = Stream.Eat(KeywordClass);
            Name    = new NameExpr(this).Parse(true);

            if (Stream.MaybeEat(OpenParenthesis)) {
                if (!Stream.PeekIs(CloseParenthesis)) {
                    do {
                        DataMembers += AnyExpr.Parse(this);
                    } while (Stream.MaybeEat(Comma));
                }

                Stream.Eat(CloseParenthesis);
            }

            // TODO: add generic classes
            if (Stream.MaybeEat(LeftArrow)) {
                List<(TypeName type, NameExpr? label)> types =
                    TypeName.ParseNamedTypeArgs(this);
                foreach ((TypeName type, NameExpr? typeLabel) in types) {
                    if (typeLabel == null) {
                        Bases += type;
                    }
                    else {
                        Keywords += type;
                    }
                }
            }

            Scope = new ScopeExpr(this);
            if (Stream.PeekIs(Spec.ScopeStartMarks)) {
                Scope.Parse();
            }

            return this;
        }
    }
}
