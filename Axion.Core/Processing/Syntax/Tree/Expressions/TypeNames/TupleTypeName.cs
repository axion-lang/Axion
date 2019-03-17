using System;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class TupleTypeName : TypeName {
        public readonly TypeName[] Types;

        public TupleTypeName(TypeName[] types) {
            Types = types ?? throw new ArgumentNullException(nameof(types));
        }

        public TupleTypeName(TypeName[] types, Token start, Token end) : this(types) {
            MarkPosition(start, end);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c += "(";
            c.AppendJoin(", ", Types);
            return c + ")";
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c += "(";
            c.AppendJoin(", ", Types);
            return c + ")";
        }
    }
}