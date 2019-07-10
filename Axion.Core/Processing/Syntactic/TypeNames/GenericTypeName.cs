using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.TypeNames {
    /// <summary>
    ///     <c>
    ///         generic_type:
    ///             type type_args;
    ///         type_args:
    ///             '[' type {',' type} ']';
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
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        public GenericTypeName(
            Expression parent,
            TypeName   target
        ) {
            Construct(parent, target, () => {
                TypeArguments = new NodeList<TypeName>(this);
                Target        = target;
                Eat(OpenBracket);
                do {
                    TypeArguments.Add(ParseTypeName());
                } while (MaybeEat(Comma));

                Eat(CloseBracket);
            });
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        public GenericTypeName(
            Expression        parent,
            GenericNameSyntax csNode
        ) : base(parent) {
            Target = new SimpleTypeName(csNode.Identifier.Text);
            TypeArguments = new NodeList<TypeName>(
                this,
                csNode.TypeArgumentList.Arguments.Select(FromCSharp)
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