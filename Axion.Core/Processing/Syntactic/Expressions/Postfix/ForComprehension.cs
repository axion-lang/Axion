using Axion.Core.Processing.Lexical.Tokens;
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
        private Expr target = null!;

        [NoPathTraversing]
        public Expr Target {
            get => target;
            set => target = Bind(value);
        }

        private Expr item = null!;

        public Expr Item {
            get => item;
            set => item = Bind(value);
        }

        private Expr iterable = null!;

        public Expr Iterable {
            get => iterable;
            set => iterable = Bind(value);
        }

        private NodeList<Expr> conditions = null!;

        public NodeList<Expr> Conditions {
            get => conditions;
            set => conditions = Bind(value);
        }

        private Expr? right;

        public Expr? Right {
            get => right;
            set => right = BindNullable(value);
        }

        public bool IsGenerator;
        public bool IsNested;

        [NoPathTraversing]
        public override TypeName ValueType => Target.ValueType;

        public ForComprehension(Node parent) : base(parent) { }

        public ForComprehension Parse() {
            Conditions ??= new NodeList<Expr>(this);
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
                                new UnaryExpr(this) {
                                    Operator = new OperatorToken(Source, tokenType: OpNot),
                                    Value    = Parse(this)
                                }
                            );
                        }
                    }

                    if (Stream.PeekIs(KeywordFor)) {
                        Right = new ForComprehension(Parent) {
                            Target = this, IsNested = true
                        }.Parse();
                    }
                }
            );
            return this;
        }
    }
}
