using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         class-def:
    ///             'class' simple-name
    ///             ['[' type-parameter [{',' type-parameter}] ']']
    ///             ['&lt;-' type [{',' type}]]
    ///             scope;
    ///     </c>
    /// </summary>
    public class ClassDef : Node, IDefinitionExpr, IDecorableExpr {
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

        private NodeList<TypeName>? typeParameters;

        public NodeList<TypeName> TypeParameters {
            get => InitIfNull(ref typeParameters);
            set => typeParameters = Bind(value);
        }

        private NodeList<TypeName>? bases;

        public NodeList<TypeName> Bases {
            get => InitIfNull(ref bases);
            set => bases = Bind(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        private NodeList<Node>? dataMembers;

        public NodeList<Node> DataMembers {
            get => InitIfNull(ref dataMembers);
            set => dataMembers = Bind(value);
        }

        public ClassDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Node[] items) {
            return new(Parent) {
                Target     = this,
                Decorators = new NodeList<Node>(this, items)
            };
        }

        public ClassDef WithScope(params Node[] items) {
            return WithScope((IEnumerable<Node>) items);
        }

        public ClassDef WithScope(IEnumerable<Node> items) {
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
            // generic type parameters list
            if (Stream.PeekIs(OpenBracket)) {
                TypeParameters = TypeName.ParseGenericTypeParametersList(this);
            }
            // base classes list
            if (Stream.MaybeEat(LeftArrow)) {
                // TODO: validation for previously declared generic types
                do {
                    Bases.Add(TypeName.Parse(this));
                } while (Stream.MaybeEat(Comma));
                if (Bases.Count == 0) {
                    LanguageReport.To(
                        BlameType.RedundantEmptyListOfTypeArguments,
                        Stream.Token
                    );
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
