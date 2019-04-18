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
    public class NameExpression : Expression {
        public List<string> Qualifiers { get; } = new List<string>();

        public NameExpression(SyntaxTreeNode parent, string name) {
            Parent = parent;
            Qualifiers.Add(name);
        }

        internal NameExpression(SyntaxTreeNode parent, NameSyntax csNode) : base(parent) {
            Qualifiers.AddRange(
                csNode
                    .ToString()
                    .Split('.')
            );
        }

        internal NameExpression(SyntaxTreeNode parent, bool needSimple = false) : base(parent) {
            MarkStart(Peek);
            do {
                Eat(TokenType.Identifier);
                Qualifiers.Add(Token.Value);
            } while (MaybeEat(TokenType.Dot));

            MarkEnd(Token);

            if (needSimple && Qualifiers.Count > 1) {
                Unit.ReportError("Simple name expected.", this);
            }
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            string value = string.Join(".", Qualifiers);
            if (value == "self") {
                c.Write("this");
                return;
            }
            c.Write(value);
        }
    }
}