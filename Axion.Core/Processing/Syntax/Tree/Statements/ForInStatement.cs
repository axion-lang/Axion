using System;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ForInStatement : LoopStatement {
        public  Expression Left { get; }
        private Expression list;

        public Expression List {
            get => list;
            set {
                value.Parent = this;
                list         = value;
            }
        }

        public ForInStatement(
            Token          startToken,
            Expression     left,
            Expression     list,
            BlockStatement block,
            BlockStatement noBreakBlock
        ) : base(startToken, block, noBreakBlock) {
            Left = left;
            List = list ?? throw new ArgumentNullException(nameof(list));
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c
                 + "for "
                 + Left
                 + " in "
                 + List
                 + " "
                 + Block;
            if (NoBreakBlock != null) {
                c = c + " nobreak " + NoBreakBlock;
            }

            return c;
        }
    }
}