using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.MacroPatterns;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         macro_def:
    ///             'macro' simple_name block;
    ///     </c>
    /// TODO fix macro def syntax
    /// </summary>
    public class MacroDef : Expr, IDefinitionExpr {
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private BlockExpr block;

        public BlockExpr Block {
            get => block;
            set => SetNode(ref block, value);
        }

        public CascadePattern Syntax { get; }
        internal MacroDef(Expr parent) : base(parent) { }

        public MacroDef Parse() {
            SetSpan(() => {
                Name  = new NameExpr(this).Parse(true);
                Block = new BlockExpr(this).Parse();
            });
            return this;
        }

        internal MacroDef(params IPattern[] patterns) {
            Syntax = new CascadePattern(patterns);
        }

        public override void ToAxion(CodeWriter c) {
            throw new NotSupportedException();
        }

        public override void ToCSharp(CodeWriter c) {
            throw new NotSupportedException();
        }
    }
}