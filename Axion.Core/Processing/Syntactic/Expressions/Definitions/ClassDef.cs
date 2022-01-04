using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions;

/// <summary>
///     <code>
///         class-def:
///             'class' name
///             [ type-parameter [{',' type-parameter}] [','] ]
///             [ '&lt;-' base [{',' base}] [','] ]
///             [ data-member [{',' data-member}] [','] ]
///             [ scope ];
///     </code>
/// </summary>
[Branch]
public partial class ClassDef : Node, IDefinitionExpr, IDecorableExpr {
    [Leaf] NodeList<TypeName, Ast>? bases;
    [Leaf] NodeList<Node, Ast>? dataMembers;
    [Leaf] Token? kwClass;
    [Leaf] NameExpr? name;
    [Leaf] ScopeExpr scope = null!;
    [Leaf] NodeList<TypeName, Ast>? typeParameters;

    public ClassDef(Node? parent) : base(parent) { }

    public DecoratedExpr WithDecorators(params Node[] items) {
        return new(Parent) {
            Target     = this,
            Decorators = new NodeList<Node, Ast>(this, items)
        };
    }

    public ClassDef WithScope(IEnumerable<Node> items) {
        Scope = new ScopeExpr(this).WithItems(items);
        return this;
    }

    public ClassDef Parse() {
        KwClass = Stream.Eat(KeywordClass);
        Name    = new NameExpr(this).Parse(true);
        if (Stream.MaybeEat(OpenParenthesis)) {
            if (!Stream.PeekIs(CloseParenthesis)) {
                do {
                    DataMembers += AnyExpr.Parse(this);
                } while (Stream.MaybeEat(Comma));
            }

            Stream.Eat(CloseParenthesis);
        }
        // generic type parameters list
        if (Stream.PeekIs(OpenBracket)) {
            TypeParameters = TypeName.ParseGenericTypeParametersList(this);
        }
        // base classes list
        if (Stream.MaybeEat(LeftArrow)) {
            // TODO: validation for previously declared generic types
            do {
                Bases.Add(TypeName.Parse(this));
            } while (Stream.MaybeEat(Comma));
            if (Bases.Count == 0) {
                LanguageReport.To(
                    BlameType.RedundantEmptyListOfTypeArguments,
                    Stream.Token
                );
            }
        }
        Scope = new ScopeExpr(this);
        if (Stream.PeekIs(Spec.ScopeStartMarks)) {
            Scope.Parse();
        }
        return this;
    }
}
