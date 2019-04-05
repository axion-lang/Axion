using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         union_type ::=
    ///             type ('|' type)+
    ///     </c>
    /// </summary>
    public class UnionTypeName : TypeName {
        private TypeName left;

        [NotNull]
        public TypeName Left {
            get => left;
            set => SetNode(ref left, value);
        }

        private TypeName right;

        [NotNull]
        public TypeName Right {
            get => right;
            set => SetNode(ref right, value);
        }

        public UnionTypeName([NotNull] TypeName left, [NotNull] TypeName right) {
            Left  = left;
            Right = right;
            //MarkPosition(Left, Right);
        }

        public UnionTypeName([NotNull] SyntaxTreeNode parent, [NotNull] TypeName left) {
            Parent = parent;
            Left   = left;
            
            MarkStart(Left);
            
            Eat(TokenType.OpBitOr);
            Right = Parse(this);
            
            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Left + " | " + Right;
        }
    }
}