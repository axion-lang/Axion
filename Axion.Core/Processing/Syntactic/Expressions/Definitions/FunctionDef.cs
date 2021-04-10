using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.SourceGenerators;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <code>
    ///         func-def:
    ///             'fn' [name]
    ///             ['[' type-parameter [{',' type-parameter}] ']']
    ///             ['(' [multiple-parameters] ')']
    ///             ['->' type] scope;
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class FunctionDef : AtomExpr, IDefinitionExpr, IDecorableExpr {
        [LeafSyntaxNode] Token? kwFn;
        [LeafSyntaxNode] NameExpr? name;
        [LeafSyntaxNode] NodeList<TypeName>? typeParameters;
        [LeafSyntaxNode] NodeList<FunctionParameter>? parameters;
        [LeafSyntaxNode] ScopeExpr scope = null!;

        public override TypeName? ValueType {
            get {
                try {
                    var returns = Scope.FindItemsOfType<ReturnExpr>();
                    // TODO: handle all possible returns (type unions)
                    if (returns.Count > 0) {
                        return returns[0].item.ValueType;
                    }

                    return new TupleTypeName(this);
                }
                catch {
                    return new SimpleTypeName(this, Spec.UnknownType);
                }
            }
        }

        public FunctionDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Node[] items) {
            return new(Parent) {
                Target     = this,
                Decorators = new NodeList<Node>(this, items)
            };
        }

        public FunctionDef WithScope(params Node[] items) {
            return WithScope((IEnumerable<Node>) items);
        }

        public FunctionDef WithScope(IEnumerable<Node> items) {
            Scope = new ScopeExpr(this).WithItems(items);
            return this;
        }

        public FunctionDef WithParameters(params FunctionParameter[] items) {
            Parameters = new NodeList<FunctionParameter>(this, items);
            return this;
        }

        public FunctionDef Parse(bool anonymous = false) {
            KwFn = Stream.Eat(KeywordFn);
            // name
            if (!anonymous) {
                Name = new NameExpr(this).Parse();
            }
            // generic type parameters list
            if (Stream.PeekIs(OpenBracket)) {
                TypeParameters = TypeName.ParseGenericTypeParametersList(this);
            }
            // parameters
            if (Stream.MaybeEat(OpenParenthesis)) {
                // TODO: reworking parameter lists
                Parameters = FunctionParameter.ParseList(
                    this,
                    CloseParenthesis
                );
                Stream.Eat(CloseParenthesis);
            }
            // return type
            if (Stream.MaybeEat(RightArrow)) {
                ValueType = TypeName.Parse(this);
            }
            // scope
            Scope = new ScopeExpr(this);
            if (Stream.PeekIs(Spec.ScopeStartMarks)) {
                Scope.Parse();
            }
            // single expression: `fn (arg, ...) expr`
            else if (anonymous) {
                Scope.Items += AnyExpr.Parse(Scope);
            }
            return this;
        }
    }
}
