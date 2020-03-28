using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
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
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        private NodeList<TypeName> bases;

        public NodeList<TypeName> Bases {
            get => bases;
            set => bases = Bind(value);
        }

        private NodeList<Expr> keywords;

        public NodeList<Expr> Keywords {
            get => keywords;
            set => keywords = Bind(value);
        }

        private ScopeExpr scope;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        private NodeList<Expr> dataMembers;

        public NodeList<Expr> DataMembers {
            get => dataMembers;
            set => dataMembers = Bind(value);
        }

        public ClassDef(
            string?                name     = null,
            IEnumerable<TypeName>? bases    = null,
            IEnumerable<Expr>?     keywords = null,
            ScopeExpr?             scope    = null
        ) : this(
            null, new NameExpr(name), bases, keywords,
            scope
        ) { }

        public ClassDef(
            Expr?                  parent   = null,
            NameExpr?              name     = null,
            IEnumerable<TypeName>? bases    = null,
            IEnumerable<Expr>?     keywords = null,
            ScopeExpr?             scope    = null
        ) : base(
            parent
         ?? GetParentFromChildren(name, scope)
        ) {
            Name        = name;
            Bases       = NodeList<TypeName>.From(this, bases);
            Keywords    = NodeList<Expr>.From(this, keywords);
            DataMembers = new NodeList<Expr>(this);
            Scope       = scope;
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

        public override void ToAxion(CodeWriter c) {
            c.Write("class ", Name);
            if (DataMembers.Count > 0) {
                c.Write("(");
                c.AddJoin(", ", DataMembers);
                c.Write(")");
            }
            if (Bases.Count > 0) {
                c.Write(" <- ");
                c.AddJoin(", ", Bases);
            }

            c.Write(Scope);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("public class ", Name);
            if (Bases.Count > 0) {
                c.Write(" : ");
                c.AddJoin(", ", Bases);
            }
            c.WriteLine();
            c.Write(Scope);
        }

        public override void ToPython(CodeWriter c) {
            c.Write("class ", Name);
            if (Bases.Count > 0) {
                c.Write("(");
                c.AddJoin(", ", Bases);
                c.Write(")");
            }

            c.Write(Scope);
        }
    }
}