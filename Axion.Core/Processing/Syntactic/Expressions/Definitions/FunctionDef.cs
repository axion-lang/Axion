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
            set => name = Bind(value);
        }

        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => returnType = Bind(value);
        }

        private NodeList<FunctionParameter> parameters;

        public NodeList<FunctionParameter> Parameters {
            get => parameters;
            set => parameters = Bind(value);
        }

        private ScopeExpr scope;

        public ScopeExpr Scope {
            get => scope;
            set => scope = Bind(value);
        }

        [NoTraversePath]
        public override TypeName ValueType {
            get {
                if (ReturnType != null) {
                    return ReturnType;
                }

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

        public FunctionDef(
            string?                         name       = null,
            IEnumerable<FunctionParameter>? parameters = null,
            TypeName?                       returnType = null,
            ScopeExpr?                      scope      = null
        ) : this(
            null, new NameExpr(name), parameters, returnType,
            scope
        ) { }

        public FunctionDef(
            Expr?                           parent     = null,
            NameExpr?                       name       = null,
            IEnumerable<FunctionParameter>? parameters = null,
            TypeName?                       returnType = null,
            ScopeExpr?                      scope      = null
        ) : base(
            parent
         ?? GetParentFromChildren(name, returnType, scope)
        ) {
            Name       = name;
            Parameters = NodeList<FunctionParameter>.From(this, parameters);
            ReturnType = returnType;
            Scope      = scope;
        }

        public FunctionDef Parse(bool anonymous = false) {
            SetSpan(
                () => {
                    Stream.Eat(KeywordFn);
                    if (!anonymous) {
                        Name = new NameExpr(this).Parse();
                    }

                    // parameters
                    if (Stream.MaybeEat(OpenParenthesis)) {
                        Parameters = FunctionParameter.ParseList(
                            this,
                            CloseParenthesis
                        );
                        Stream.Eat(CloseParenthesis);
                    }
                    else {
                        Parameters = new NodeList<FunctionParameter>(this);
                    }

                    // return type
                    if (Stream.MaybeEat(RightArrow)) {
                        ReturnType = TypeName.Parse(this);
                    }

                    if (Stream.PeekIs(Spec.ScopeStartMarks)) {
                        Scope = new ScopeExpr(this).Parse(
                            anonymous
                                ? ScopeType.Lambda
                                : ScopeType.Default
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