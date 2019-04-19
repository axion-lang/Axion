using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///          target 'for' target_list 'in' operation [right]
    ///     </c>
    /// </summary>
    public class ForComprehension : Expression {
        #region Properties

        private Expression target;

        [JsonIgnore]
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

            MarkStart(TokenType.KeywordFor);
            Item = ParseMultiple(this, ParsePrimaryExpr, typeof(NameExpression));
            Eat(TokenType.KeywordIn);
            Iterable = ParseMultiple(parent, ParseTestExpr, Spec.TestExprs);

            if (Peek.Is(TokenType.KeywordFor)) {
                Right = new ForComprehension(Parent, this);
            }
            else if (Peek.Is(TokenType.KeywordIf, TokenType.KeywordUnless)) {
                Right = new ConditionalComprehension(this);
            }

            MarkEnd(Token);
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