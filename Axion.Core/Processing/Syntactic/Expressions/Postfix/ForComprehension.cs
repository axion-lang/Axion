using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         for_comprehension:
    ///             'for' simple_name_list 'in' preglobal_expr [comprehension];
    ///     </c>
    /// </summary>
    public class ForComprehension : Expr {
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

        public bool IsGenerator;
        public bool IsNested;

        [NoTraversePath]
        public override TypeName ValueType => Target.ValueType;

        public ForComprehension(
            Expr parent   = null,
            Expr target   = null,
            bool isNested = false
        ) : base(parent) {
            IsNested   = isNested;
            Target     = target;
            Conditions = new NodeList<Expr>(this);
        }

        public ForComprehension Parse() {
            SetSpan(() => {
                if (Target == null && !IsNested) {
                    Target = Parsing.ParseInfix(this);
                }

                Stream.Eat(KeywordFor);
                Item = Parsing.ParseMultiple(
                    this,
                    Parsing.ParseAtom,
                    typeof(NameExpr)
                );
                Stream.Eat(OpIn);
                Iterable = Parsing.ParseMultiple(this, expectedTypes: typeof(IInfixExpr));

                while (Stream.Peek.Is(KeywordIf, KeywordUnless)) {
                    if (Stream.MaybeEat(KeywordIf)) {
                        Conditions.Add(Parsing.ParseInfix(this));
                    }
                    else if (Stream.MaybeEat(KeywordUnless)) {
                        Conditions.Add(
                            new UnaryExpr(
                                this,
                                OpNot,
                                Parsing.ParseInfix(this)
                            )
                        );
                    }
                }

                if (Stream.Peek.Is(KeywordFor)) {
                    Right = new ForComprehension(Parent, this, true).Parse();
                }
            });
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