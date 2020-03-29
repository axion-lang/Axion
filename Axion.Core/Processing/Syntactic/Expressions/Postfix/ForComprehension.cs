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
            set => target = Bind(value);
        }

        private Expr item;

        public Expr Item {
            get => item;
            set => item = Bind(value);
        }

        private Expr iterable;

        public Expr Iterable {
            get => iterable;
            set => iterable = Bind(value);
        }

        private NodeList<Expr> conditions;

        public NodeList<Expr> Conditions {
            get => conditions;
            set => conditions = Bind(value);
        }

        private Expr right;

        public Expr Right {
            get => right;
            set => right = Bind(value);
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
    }
}