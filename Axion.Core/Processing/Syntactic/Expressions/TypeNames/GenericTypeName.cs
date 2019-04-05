using System;
using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         generic_type ::=
    ///             type '&lt;' type {',' type} '&gt;'
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

        public GenericTypeName(
            [NotNull] TypeName              target,
            [NotNull] IEnumerable<TypeName> typeArguments
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

        public GenericTypeName(
            SyntaxTreeNode     parent,
            [NotNull] TypeName target
        ) {
            Parent = parent;
            Target = target;
            TypeArguments = new NodeList<TypeName>(this);

            MarkStart(Target);
            
            Eat(TokenType.OpLess);
            do {
                TypeArguments.Add(Parse(parent));
            } while (MaybeEat(TokenType.Comma));
            Eat(TokenType.OpGreater);
            
            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + target + "<";
            c.AppendJoin(",", typeArguments);
            return c + ">";
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + target + "<";
            c.AppendJoin(",", typeArguments);
            return c + ">";
        }
    }
}