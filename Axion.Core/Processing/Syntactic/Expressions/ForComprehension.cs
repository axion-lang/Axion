using System;
using Axion.Core.Processing.Lexical.Tokens;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///          parent 'for' target_list 'in' operation [right]
    ///     </c>
    /// </summary>
    public class ForComprehension : Expression {
        private Expression item;

        public Expression Item {
            get => item;
            set => SetNode(ref item, value);
        }

        private Expression iterable;

        public Expression Iterable {
            get => iterable;
            set => SetNode(ref iterable, value);
        }

        private Expression right;

        public Expression Right {
            get => right;
            set => SetNode(ref right, value);
        }

        public ForComprehension([NotNull] Expression left, Expression item, Expression iterable) {
            Parent   = left ?? throw new ArgumentNullException(nameof(left));
            Item     = item ?? throw new ArgumentNullException(nameof(item));
            Iterable = iterable ?? throw new ArgumentNullException(nameof(iterable));
        }

        public ForComprehension(SyntaxTreeNode parent) {
            Parent = parent;

            StartNode(TokenType.KeywordFor);
            Item = MaybeTuple(
                TargetList(this, out bool trailingComma),
                trailingComma
            );
            Eat(TokenType.OpIn);
            Iterable = ParseOperation(this);

            if (PeekIs(TokenType.KeywordFor)) {
                Right = new ForComprehension(this);
            }
            else if (PeekIs(TokenType.KeywordIf, TokenType.KeywordUnless)) {
                Right = new ConditionalComprehension(this);
            }
        }
    }
}