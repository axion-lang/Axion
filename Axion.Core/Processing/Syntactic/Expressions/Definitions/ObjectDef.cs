using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         object_def:
    ///             'object' simple_name ['&lt;' type_arg_list] block;
    ///     </c>
    /// </summary>
    public class ObjectDef : Expr, IDefinitionExpr, IDecoratedExpr {
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

        internal ObjectDef(Expr parent) : base(parent) {
            Bases = new NodeList<TypeName>(this);
            SetSpan(() => {
                Stream.Eat(KeywordObject);

                Name = new NameExpr(this);
                if (Stream.MaybeEat(LeftArrow)) {
                    List<(TypeName, NameExpr)> types = new TypeName(this).ParseNamedTypeArgs();
                    foreach ((TypeName type, NameExpr typeLabel) in types) {
                        if (typeLabel == null) {
                            Bases.Add(type);
                        }
                    }
                }

                Block = new BlockExpr(this).Parse();
            });
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("object ", Name, " ", Block);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("public class ", Name, " ", Block);
        }
    }
}