using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         code_quote_expr:
    ///             '{{' expr '}}';
    ///     </c>
    /// </summary>
    public class CodeQuoteExpr : Expr {
        private BlockExpr block;

        public BlockExpr Block {
            get => block;
            set => SetNode(ref block, value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Block.ValueType;

        public CodeQuoteExpr(
            Expr      parent = null,
            BlockExpr block  = null
        ) : base(parent) {
            Block = block ?? new BlockExpr(this);
        }

        public CodeQuoteExpr Parse() {
            SetSpan(() => {
                Stream.Eat(DoubleOpenBrace);
                while (!Stream.PeekIs(DoubleCloseBrace, TokenType.End)) {
                    Block.Items.Add(AnyExpr.Parse(this));
                }

                Stream.Eat(DoubleCloseBrace);
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("{{", Block, "}}");
        }
    }
}