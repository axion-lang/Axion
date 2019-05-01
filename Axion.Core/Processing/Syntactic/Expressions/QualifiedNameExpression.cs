using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         qualified_name:
    ///             ID ('.' ID)+
    ///     </c>
    /// </summary>
    public class QualifiedNameExpression : NameExpression {
        public          List<string> Qualifiers { get; } = new List<string>();
        public override string       Name       => string.Join(".", Qualifiers);

        public QualifiedNameExpression(SyntaxTreeNode parent, IEnumerable<string> qualifiers) {
            Parent     = parent;
            Qualifiers = qualifiers.ToList();
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
                Eat(Identifier);
                Qualifiers.Add(Token.Value);
            } while (MaybeEat(Dot));

            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }
    }
}