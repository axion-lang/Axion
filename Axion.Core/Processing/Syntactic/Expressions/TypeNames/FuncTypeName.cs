using Axion.Core.Processing.Lexical.Tokens;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

/// <summary>
///     <code>
///         func-type:
///             type '->' type
///     </code>
/// </summary>
[Branch]
public partial class FuncTypeName : TypeName {
    [Leaf] TypeName? argsType;
    [Leaf] Token? joiningMark;
    [Leaf] TypeName? returnType;

    public FuncTypeName(Node parent) : base(parent) { }

    public FuncTypeName Parse() {
        ArgsType    ??= Parse(this);
        JoiningMark =   Stream.Eat(RightArrow);
        ReturnType  =   Parse(this);
        return this;
    }
}
