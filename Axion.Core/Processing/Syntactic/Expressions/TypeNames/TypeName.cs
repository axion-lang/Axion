using Axion.Core.Processing.Errors;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames;

/// <summary>
///     <code>
///         type: simple-type  | tuple-type
///             | generic-type | array-type
///             | union-type   | func-type;
///     </code>
/// </summary>
[Branch]
public partial class TypeName : Node {
    protected TypeName(Node? parent) : base(parent) { }

    internal static TypeName Parse(Node parent) {
        var s = parent.Unit.TokenStream;
        // leading
        TypeName leftTypeName;
        // tuple
        if (s.PeekIs(OpenParenthesis)) {
            var tuple = new TupleTypeName(parent).Parse();
            leftTypeName = tuple.Types.Count == 1 ? tuple.Types[0] : tuple;
        }
        // simple
        else if (s.PeekIs(Identifier)) {
            leftTypeName = new SimpleTypeName(parent).Parse();
        }
        else {
            LanguageReport.UnexpectedSyntax(Identifier, s.Peek);
            return new SimpleTypeName(parent, "UnknownType");
        }

        if (s.PeekIs(Colon) && leftTypeName is SimpleTypeName st) {
            return new GenericParameterTypeName(parent) {
                Name = st.Name
            }.Parse();
        }

        // middle
        // generic ('[' followed by not ']')
        if (s.PeekIs(OpenBracket) && !s.PeekByIs(2, CloseBracket)) {
            leftTypeName = new GenericTypeName(parent) {
                Target = leftTypeName
            }.Parse();
        }

        // array
        if (s.PeekIs(OpenBracket)) {
            leftTypeName = new ArrayTypeName(parent) {
                ElementType = leftTypeName
            }.Parse();
        }

        // trailing
        // union
        if (s.PeekIs(Pipe)) {
            leftTypeName = new UnionTypeName(parent) {
                Left = leftTypeName
            }.Parse();
        }

        if (s.PeekIs(RightArrow)) {
            leftTypeName = new FuncTypeName(parent) {
                ArgsType = leftTypeName
            }.Parse();
        }

        return leftTypeName;
    }

    internal static NodeList<TypeName, Ast> ParseGenericTypeParametersList(
        Node parent
    ) {
        var s = parent.Stream;
        var tps = new NodeList<TypeName, Ast>(parent);
        s.Eat(OpenBracket);
        do {
            var type = Parse(parent);
            if (type is not ITypeParameter) {
                LanguageReport.To(BlameType.ExpectedTypeParameter, type);
            }
            tps.Add(type);
        } while (s.MaybeEat(Semicolon));
        if (tps.Count == 0) {
            LanguageReport.To(
                BlameType.RedundantParentheses,
                s.Token
            );
        }
        s.MaybeEat(Comma);
        s.Eat(CloseBracket);
        return tps;
    }
}
