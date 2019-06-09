using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         qualified_name:
    ///             ID ('.' ID)+
    ///     </c>
    /// </summary>
    public class QualifiedNameExpression : NameExpression {
        public          List<string> Qualifiers { get; } = new List<string>();
        public override string       Name       => string.Join(".", Qualifiers);

        public QualifiedNameExpression(
            AstNode             parent,
            IEnumerable<string> qualifiers
        ) : base(parent) {
            Qualifiers = qualifiers.ToList();
        }

        internal QualifiedNameExpression(AstNode parent, NameSyntax csNode) : base(parent) {
            Qualifiers.AddRange(
                csNode
                    .ToString()
                    .Split('.')
            );
        }

        internal QualifiedNameExpression(AstNode parent) : base(parent) {
            MarkStart(Peek);
            do {
                Eat(TokenType.Identifier);
                Qualifiers.Add(Token.Value);
            } while (MaybeEat(TokenType.Dot));

            MarkEnd();
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }
    }
}