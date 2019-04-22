using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         for_comprehension:
    ///             target 'for' ((name)) 'in' test [comprehension]
    ///     </c>
    /// </summary>
    public class ForComprehension : Expression {
        #region Properties

        private Expression target;

        public Expression Target {
            get => target;
            set => SetNode(ref target, value);
        }

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

        internal override TypeName ValueType => Parent.ValueType;

        #endregion

        public ForComprehension(SyntaxTreeNode parent, Expression target) : base(parent) {
            Target = target;

            MarkStart(target);
            Eat(TokenType.KeywordFor);
            Item = ParseExpression(this, ParsePrimaryExpr, typeof(SimpleNameExpression));
            Eat(TokenType.OpIn);
            Iterable = ParseExpression(parent, expectedTypes: Spec.TestExprs);

            if (Peek.Is(TokenType.KeywordFor)) {
                Right = new ForComprehension(Parent, this);
            }
            else if (Peek.Is(TokenType.KeywordIf, TokenType.KeywordUnless)) {
                Right = new ConditionalComprehension(this);
            }

            MarkEnd(Token);
        }

        public ForComprehension(
            Expression target,
            Expression item,
            Expression iterable,
            Expression rightComprehension = null
        ) {
            Target   = target;
            Item     = item;
            Iterable = iterable;
            Right    = rightComprehension;
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(
                Target,
                " for ",
                Item,
                " in ",
                Iterable,
                Right
            );
        }

        public override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}