using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         for-comprehension:
    ///             'for' multiple-name 'in' multiple-infix (('if'|'unless') condition)* [for-comprehension];
    ///     </c>
    /// </summary>
    public class ForComprehension : InfixExpr {
        private Node? target;

        [NoPathTraversing]
        public Node? Target {
            get => target;
            set => target = BindNullable(value);
        }

        private Node item = null!;

        public Node Item {
            get => item;
            set => item = Bind(value);
        }

        private Node iterable = null!;

        public Node Iterable {
            get => iterable;
            set => iterable = Bind(value);
        }

        private NodeList<Node>? conditions;

        public NodeList<Node> Conditions {
            get => InitIfNull(ref conditions);
            set => conditions = Bind(value);
        }

        private Node? right;

        public Node? Right {
            get => right;
            set => right = BindNullable(value);
        }

        public bool IsNested { get; private init; }

        public override TypeName? ValueType => Target?.ValueType;

        public ForComprehension(Node parent) : base(parent) { }

        public ForComprehension Parse() {
            if (Target == null && !IsNested) {
                Target = Parse(this);
            }

            Stream.Eat(KeywordFor);
            Item = Multiple.Parse<AtomExpr>(this);
            Stream.Eat(In);
            Iterable = Multiple.Parse<InfixExpr>(this);

            while (Stream.PeekIs(KeywordIf, KeywordUnless)) {
                if (Stream.MaybeEat(KeywordIf)) {
                    Conditions += Parse(this);
                }
                else if (Stream.MaybeEat(KeywordUnless)) {
                    Conditions += new UnaryExpr(this) {
                        Operator = new OperatorToken(
                            Unit,
                            tokenType: Not
                        ),
                        Value = Parse(this)
                    };
                }
            }

            if (Stream.PeekIs(KeywordFor)) {
                Right = new ForComprehension(Parent!) {
                    Target   = this,
                    IsNested = true
                }.Parse();
            }
            return this;
        }
    }
}
