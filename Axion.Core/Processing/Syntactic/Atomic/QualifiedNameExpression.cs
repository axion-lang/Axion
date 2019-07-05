using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Atomic {
    /// <summary>
    ///     <c>
    ///         qualified_name:
    ///             ID ('.' ID)+;
    ///     </c>
    /// </summary>
    public class QualifiedNameExpression : NameExpression {
        public readonly string[] Qualifiers;

        public override string Name {
            get => string.Join(".", Qualifiers);
            set => throw new NotImplementedException();
        }

        public QualifiedNameExpression(
            Expression          parent,
            IEnumerable<string> qualifiers
        ) : base(parent) {
            Qualifiers = qualifiers.ToArray();
        }

        internal QualifiedNameExpression(Expression parent, NameSyntax csNode) : base(parent) {
            Qualifiers =
                csNode
                    .ToString()
                    .Split('.');
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(string.Join(".", Qualifiers));
        }
    }
}