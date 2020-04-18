using System.Collections.Generic;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         func-def:
    ///             'fn' [name] ['(' [multiple-parameters] ')'] ['->' type] scope;
    ///     </c>
    /// </summary>
    public class FunctionDef : AtomExpr, IDefinitionExpr {
        private NameExpr? name;

        public NameExpr? Name {
            get => name;
            set => name = BindNullable(value);
        }

        private NodeList<FunctionParameter> parameters = null!;

        public NodeList<FunctionParameter> Parameters {
            get => parameters;
            set => parameters = Bind(value);
        }

        private ScopeExpr scope = null!;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        [NoPathTraversing]
        public override TypeName ValueType {
            get {
                try {
                    List<(ReturnExpr item, ScopeExpr itemParentScope, int itemIndex)> returns =
                        Scope.FindItemsOfType<ReturnExpr>();
                    // TODO: handle all possible returns (type unions)
                    if (returns.Count > 0) {
                        return returns[0].item.ValueType;
                    }

                    return new SimpleTypeName("void");
                }
                catch {
                    return new SimpleTypeName("UNKNOWN_TYPE");
                }
            }
        }

        public FunctionDef(Expr parent) : base(parent) { }

        public FunctionDef Parse(bool anonymous = false) {
            SetSpan(
                () => {
                    Stream.Eat(KeywordFn);
                    if (!anonymous) {
                        Name = new NameExpr(this).Parse();
                    }

                    // parameters
                    if (Stream.MaybeEat(OpenParenthesis)) {
                        Parameters = FunctionParameter.ParseList(this, CloseParenthesis);
                        Stream.Eat(CloseParenthesis);
                    }
                    else {
                        Parameters = new NodeList<FunctionParameter>(this);
                    }

                    // return type
                    if (Stream.MaybeEat(RightArrow)) {
                        ValueType = TypeName.Parse(this);
                    }

                    if (Stream.PeekIs(Spec.ScopeStartMarks)) {
                        Scope = new ScopeExpr(this).Parse(
                            anonymous ? ScopeType.Lambda : ScopeType.Default
                        );
                    }
                    else {
                        Scope = new ScopeExpr(this);
                    }
                }
            );
            return this;
        }
    }
}
