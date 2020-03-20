using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         for-comprehension:
    ///             'for' multiple-name 'in' multiple-infix (('if'|'unless') condition)* [for-comprehension];
    ///     </c>
    /// </summary>
    public class ForComprehension : InfixExpr {
        private Expr target;

        [NoTraversePath]
        public Expr Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private Expr item;

        public Expr Item {
            get => item;
            set => SetNode(ref item, value);
        }

        private Expr iterable;

        public Expr Iterable {
            get => iterable;
            set => SetNode(ref iterable, value);
        }

        private NodeList<Expr> conditions;

        public NodeList<Expr> Conditions {
            get => conditions;
            set => SetNode(ref conditions, value);
        }

        private Expr right;

        public Expr Right {
            get => right;
            set => SetNode(ref right, value);
        }

        public          bool IsGenerator;
        public readonly bool IsNested;

        [NoTraversePath]
        public override TypeName ValueType => Target.ValueType;

        public ForComprehension(
            Expr? parent   = null,
            Expr? target   = null,
            bool  isNested = false
        ) : base(
            parent
         ?? GetParentFromChildren(target)
        ) {
            IsNested   = isNested;
            Target     = target;
            Conditions = new NodeList<Expr>(this);
        }

        public ForComprehension Parse() {
            SetSpan(
                () => {
                    if (Target == null && !IsNested) {
                        Target = Parse(this);
                    }

                    Stream.Eat(KeywordFor);
                    Item = Multiple<AtomExpr>.Parse(this);
                    Stream.Eat(OpIn);
                    Iterable = Multiple<InfixExpr>.Parse(this);

                    while (Stream.PeekIs(KeywordIf, KeywordUnless)) {
                        if (Stream.MaybeEat(KeywordIf)) {
                            Conditions.Add(Parse(this));
                        }
                        else if (Stream.MaybeEat(KeywordUnless)) {
                            Conditions.Add(
                                new UnaryExpr(
                                    this,
                                    OpNot,
                                    Parse(this)
                                )
                            );
                        }
                    }

                    if (Stream.PeekIs(KeywordFor)) {
                        Right = new ForComprehension(Parent, this, true).Parse();
                    }
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            if (!IsNested) {
                c.Write(Target);
            }

            c.Write(
                " for ",
                Item,
                " in ",
                Iterable,
                Right
            );
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("from ", Item, " in ", Iterable);
            if (Right != null) {
                c.Write(" ", Right);
            }

            if (Conditions.Count > 0) {
                c.Write(" where ");
                c.AddJoin(" && ", Conditions);
            }

            if (!IsNested) {
                c.Write(" select ", Target);
            }
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }
    }
}