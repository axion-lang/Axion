using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.MacroPatterns;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Definitions {
    /// <summary>
    ///     <c>
    ///         macro_def:
    ///             'macro' simple_name block;
    ///     </c>
    /// TODO fix macro def syntax
    /// </summary>
    public class MacroDefinition : Expression {
        private SimpleNameExpression name;

        public SimpleNameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private BlockExpression block;

        public BlockExpression Block {
            get => block;
            set => SetNode(ref block, value);
        }

        public CascadePattern Syntax { get; }

        /// <summary>
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal MacroDefinition(Expression parent) {
            Construct(parent, () => {
                Eat(KeywordMacro);

                Name  = new SimpleNameExpression(this);
                Block = new BlockExpression(this, BlockType.Named);
            });
        }

        internal MacroDefinition(params IPattern[] patterns) {
            Syntax = new CascadePattern(patterns);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}