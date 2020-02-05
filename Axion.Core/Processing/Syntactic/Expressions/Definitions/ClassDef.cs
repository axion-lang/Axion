using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         class_def:
    ///             'class' simple_name [type_args] ['&lt;-' type_arg_list] block;
    ///     </c>
    /// </summary>
    public class ClassDef : Expr, IDefinitionExpr, IDecoratedExpr {
        private Expr name;

        public Expr Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private NodeList<TypeName> bases;

        public NodeList<TypeName> Bases {
            get => bases;
            set => SetNode(ref bases, value);
        }

        private NodeList<Expr> keywords;

        public NodeList<Expr> Keywords {
            get => keywords;
            set => SetNode(ref keywords, value);
        }

        private BlockExpr block;

        public BlockExpr Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<Expr> modifiers;

        public NodeList<Expr> Modifiers {
            get => modifiers;
            set => SetNode(ref modifiers, value);
        }

        private Expr dataMembers;

        public Expr DataMembers {
            get => dataMembers;
            set => SetNode(ref dataMembers, value);
        }

        public ClassDef(
            Expr               parent    = null,
            NameExpr           name      = null,
            NodeList<TypeName> bases     = null,
            NodeList<Expr>     keywords  = null,
            BlockExpr          block     = null,
            NodeList<Expr>     modifiers = null
        ) : base(parent) {
            Name      = name;
            Bases     = bases    ?? new NodeList<TypeName>(this);
            Keywords  = keywords ?? new NodeList<Expr>(this);
            Block     = block;
            Modifiers = modifiers;
        }

        public ClassDef Parse() {
            SetSpan(() => {
                Stream.Eat(KeywordClass);
                Name = new NameExpr(this).Parse(true);

                if (Stream.PeekIs(OpenParenthesis)) {
                    DataMembers = Parsing.MultipleExprs(this, expectedTypes: typeof(NameDef));
                }

                // TODO: add generic classes
                if (Stream.MaybeEat(LeftArrow)) {
                    List<(TypeName type, NameExpr label)> types = new TypeName(this).ParseNamedTypeArgs();
                    foreach ((TypeName type, NameExpr typeLabel) in types) {
                        if (typeLabel == null) {
                            Bases.Add(type);
                        }
                        else {
                            Keywords.Add(type);
                        }
                    }
                }

                if (Stream.PeekIs(Spec.BlockStartMarks)) {
                    Block = new BlockExpr(this).Parse();
                }
                else {
                    Block = new BlockExpr(this);
                }
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("class ", Name);
            if (Bases.Count > 0) {
                c.Write(" <- ", Bases);
            }

            c.Write(Block);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("public class ", Name);
            if (Bases.Count > 0) {
                c.Write(" <- ", Bases);
            }

            c.WriteLine();
            c.Write(Block);
        }

        public override void ToPython(CodeWriter c) {
            c.Write("class ", Name);
            if (Bases.Count > 0) {
                c.Write("(", Bases, ")");
            }

            c.Write(Block);
        }
    }
}