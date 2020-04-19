using System.Collections.Generic;
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
        private NameExpr name = null!;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        private NodeList<TypeName> bases = null!;

        public NodeList<TypeName> Bases {
            get => bases;
            set => bases = Bind(value);
        }

        private NodeList<Expr> keywords = null!;

        public NodeList<Expr> Keywords {
            get => keywords;
            set => keywords = Bind(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        private NodeList<Expr> dataMembers;

        public NodeList<Expr> DataMembers {
            get => dataMembers;
            set => dataMembers = Bind(value);
        }

        public ClassDef(Node parent) : base(parent) {
            Bases       = new NodeList<TypeName>(this);
            Keywords    = new NodeList<Expr>(this);
            DataMembers = new NodeList<Expr>(this);
        }

        public ClassDef Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordClass);
                    Name = new NameExpr(this).Parse(true);

                    if (Stream.MaybeEat(OpenParenthesis)) {
                        if (!Stream.PeekIs(CloseParenthesis)) {
                            do {
                                DataMembers.Add(AnyExpr.Parse(this));
                            } while (Stream.MaybeEat(Comma));
                        }

                        Stream.Eat(CloseParenthesis);
                    }

                    // TODO: add generic classes
                    if (Stream.MaybeEat(LeftArrow)) {
                        List<(TypeName type, NameExpr label)> types =
                            TypeName.ParseNamedTypeArgs(this);
                        foreach ((TypeName type, NameExpr typeLabel) in types) {
                            if (typeLabel == null) {
                                Bases.Add(type);
                            }
                            else {
                                Keywords.Add(type);
                            }
                        }
                    }

                    Scope = new ScopeExpr(this);
                    if (Stream.PeekIs(Spec.ScopeStartMarks)) {
                        Scope.Parse();
                    }
                }
            );
            return this;
        }
    }
}
