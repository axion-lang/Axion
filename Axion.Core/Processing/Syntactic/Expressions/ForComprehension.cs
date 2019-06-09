using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         for_comprehension:
    ///             'for' simple_name_list 'in' preglobal_expr [comprehension];
    ///     </c>
    /// </summary>
    public class ForComprehension : Expression {
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

        public override TypeName ValueType => Target.ValueType;

        public ForComprehension(AstNode parent, Expression target) : base(parent) {
            MarkStart(Target = target);
            Eat(KeywordFor);
            Item = ParseMultiple(
                this,
                ParseAtomExpr,
                expectedTypes: typeof(SimpleNameExpression)
            );
            Eat(OpIn);
            Iterable = ParseMultiple(parent, expectedTypes: Spec.InfixExprs);

            if (Peek.Is(KeywordFor)) {
                Right = new ForComprehension(Parent, this);
            }
            else if (Peek.Is(KeywordIf, KeywordUnless)) {
                Right = new ConditionalComprehension(this);
            }

            MarkEnd();
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

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(
                Target,
                " for ",
                Item,
                " in ",
                Iterable,
                Right
            );
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}