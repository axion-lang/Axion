using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         generic_type:
    ///             type '[' type {',' type} ']';
    ///     </c>
    /// </summary>
    public class GenericTypeName : TypeName {
        private TypeName target;

        public TypeName Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private NodeList<TypeName> typeArguments;

        public NodeList<TypeName> TypeArguments {
            get => typeArguments;
            set => SetNode(ref typeArguments, value);
        }

        /// <summary>
        ///     Constructs expression from Axion tokens.
        /// </summary>
        public GenericTypeName(
            SyntaxTreeNode parent,
            TypeName       target
        ) {
            Parent        = parent;
            Target        = target;
            TypeArguments = new NodeList<TypeName>(this);

            MarkStart(Target);

            Eat(OpenBracket);
            do {
                TypeArguments.Add(ParseTypeName(parent));
            } while (MaybeEat(Comma));

            Eat(CloseBracket);

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        public GenericTypeName(
            SyntaxTreeNode    parent,
            GenericNameSyntax csNode
        ) {
            Parent = parent;
            Target = new SimpleTypeName(csNode.Identifier.Text);
            TypeArguments = new NodeList<TypeName>(
                this,
                csNode.TypeArgumentList.Arguments.Select(a => FromCSharp(this, a))
            );
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        public GenericTypeName(
            TypeName              target,
            IEnumerable<TypeName> typeArguments
        ) {
            Target        = target;
            TypeArguments = new NodeList<TypeName>(this, typeArguments);
            if (TypeArguments.Count == 0) {
                throw new ArgumentException(
                    "Value cannot be an empty collection.",
                    nameof(typeArguments)
                );
            }
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Target + "[");
            c.AddJoin(",", typeArguments);
            c.Write("]");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Target + "<");
            c.AddJoin(",", typeArguments);
            c.Write(">");
        }
    }
}