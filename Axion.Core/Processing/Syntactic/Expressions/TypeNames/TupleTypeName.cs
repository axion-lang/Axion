using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         tuple_type ::=
    ///             '(' [type {',' type}] ')'
    ///     </c>
    /// </summary>
    public class TupleTypeName : TypeName {
        private NodeList<TypeName> types;

        public NodeList<TypeName> Types {
            get => types;
            set => SetNode(ref types, value);
        }

        public TupleTypeName(IEnumerable<TypeName> types) {
            Types = new NodeList<TypeName>(this, types);
        }

        public TupleTypeName(SyntaxTreeNode parent) {
            Parent = parent;
            Types  = new NodeList<TypeName>(this);

            StartNode(TokenType.OpenParenthesis);
            if (!PeekIs(TokenType.CloseParenthesis)) {
                do {
                    Types.Add(Parse(parent));
                } while (MaybeEat(TokenType.Comma));
            }
            Eat(TokenType.CloseParenthesis);
            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c += "(";
            c.AppendJoin(", ", types);
            return c + ")";
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c += "(";
            c.AppendJoin(", ", types);
            return c + ")";
        }
    }
}