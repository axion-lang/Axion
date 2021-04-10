using Axion.Core.Processing.Lexical.Tokens;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <code>
    ///         func-type:
    ///             type '->' type
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class FuncTypeName : TypeName {
        [LeafSyntaxNode] TypeName? argsType;
        [LeafSyntaxNode] Token? joiningMark;
        [LeafSyntaxNode] TypeName? returnType;

        public FuncTypeName(Node parent) : base(parent) { }

        public FuncTypeName Parse() {
            ArgsType    ??= Parse(this);
            JoiningMark =   Stream.Eat(RightArrow);
            ReturnType  =   Parse(this);
            return this;
        }
    }
}
