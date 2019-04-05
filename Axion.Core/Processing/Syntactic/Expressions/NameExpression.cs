using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         name ::=
    ///             ID {'.' ID}
    ///     </c>
    /// </summary>
    public class NameExpression : Expression {
        public List<WordToken> Qualifiers { get; } = new List<WordToken>();

        public NameExpression(string name) {
            Qualifiers.Add(new WordToken(name));
        }

        public NameExpression(WordToken name) {
            Qualifiers.Add(name);
            MarkPosition(name);
        }

        internal NameExpression(SyntaxTreeNode parent, bool needSimple = false) {
            Parent = parent;
            
            MarkStart(Peek);
            do {
                Eat(TokenType.Identifier);
                Qualifiers.Add((WordToken) Token);
            } while (MaybeEat(TokenType.Dot));
            MarkEnd(Token);
            
            if (needSimple && Qualifiers.Count > 1) {
                Unit.ReportError("Simple name expected.", this);
            }
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + string.Join(".", Qualifiers);
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c.AppendJoin(".", Qualifiers);
        }
    }
}