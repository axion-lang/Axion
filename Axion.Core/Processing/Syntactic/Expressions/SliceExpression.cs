using System;
using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class SliceExpression : Expression {
        private Expression start;

        internal Expression Start {
            get => start;
            set => SetNode(ref start, value);
        }

        private Expression stop;

        public Expression Stop {
            get => stop;
            set => SetNode(ref stop, value);
        }

        private Expression step;

        public Expression Step {
            get => step;
            set => SetNode(ref step, value);
        }

        public SliceExpression(
            AstNode    parent,
            Expression start,
            Expression stop,
            Expression step
        ) : base(parent) {
            Start = start;
            Stop  = stop;
            Step  = step;

            MarkPosition(start ?? stop ?? step, step ?? stop ?? start);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(
                "[",
                Start,
                ":",
                Stop,
                ":",
                Step,
                "]"
            );
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}