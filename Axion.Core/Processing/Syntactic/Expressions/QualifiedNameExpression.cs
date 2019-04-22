using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         name:
    ///             ID {'.' ID}
    ///     </c>
    /// </summary>
    public class QualifiedNameExpression : NameExpression {
        public List<string> Qualifiers { get; } = new List<string>();

        public QualifiedNameExpression(SyntaxTreeNode parent, List<string> qualifiers) {
            Parent     = parent;
            Qualifiers = qualifiers;
        }

        internal QualifiedNameExpression(SyntaxTreeNode parent, NameSyntax csNode) : base(parent) {
            Qualifiers.AddRange(
                csNode
                    .ToString()
                    .Split('.')
            );
        }

        internal QualifiedNameExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(Peek);
            do {
                Eat(TokenType.Identifier);
                Qualifiers.Add(Token.Value);
            } while (MaybeEat(TokenType.Dot));

            MarkEnd(Token);
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }
    }
}