using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         qualified_name:
    ///             ID ('.' ID)+
    ///     </c>
    /// </summary>
    public class QualifiedNameExpression : NameExpression {
        public List<string> Qualifiers { get; } = new List<string>();

        public override string Name {
            get => string.Join(".", Qualifiers);
            set => throw new NotImplementedException();
        }

        internal QualifiedNameExpression(Expression parent) {
            Construct(parent, () => {
                do {
                    Eat(TokenType.Identifier);
                    Qualifiers.Add(Token.Value);
                } while (MaybeEat(TokenType.Dot));
            });
        }

        public QualifiedNameExpression(
            Expression          parent,
            IEnumerable<string> qualifiers
        ) : base(parent) {
            Qualifiers = qualifiers.ToList();
        }

        internal QualifiedNameExpression(Expression parent, NameSyntax csNode) : base(parent) {
            Qualifiers.AddRange(
                csNode
                    .ToString()
                    .Split('.')
            );
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }
    }
}