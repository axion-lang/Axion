using System;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class GenericTypeName : TypeName {
        public readonly TypeName   Target;
        public readonly TypeName[] TypeArguments;

        public GenericTypeName(TypeName target, TypeName[] typeArguments) {
            if (typeArguments.Length == 0) {
                throw new ArgumentException(
                    "Value cannot be an empty collection.",
                    nameof(typeArguments)
                );
            }

            Target        = target ?? throw new ArgumentNullException(nameof(target));
            TypeArguments = typeArguments ?? throw new ArgumentNullException(nameof(typeArguments));
        }

        public GenericTypeName(TypeName target, TypeName[] typeArguments, Token end) : this(
            target,
            typeArguments
        ) {
            MarkPosition(target, end);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + Target + "<";
            c.AppendJoin(",", TypeArguments);
            return c + ">";
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c = c + Target + "<";
            c.AppendJoin(",", TypeArguments);
            return c + ">";
        }
    }
}