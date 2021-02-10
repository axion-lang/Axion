using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         func-def:
    ///             'fn' [name] ['(' [multiple-parameters] ')'] ['->' type] scope;
    ///     </c>
    /// </summary>
    public class FunctionDef : AtomExpr, IDefinitionExpr, IDecorableExpr {
        private Token? kwFn;

        public Token? KwFn {
            get => kwFn;
            set => kwFn = BindNullable(value);
        }

        private NameExpr? name;

        public NameExpr? Name {
            get => name;
            set => name = BindNullable(value);
        }

        private NodeList<FunctionParameter>? parameters;

        public NodeList<FunctionParameter> Parameters {
            get => InitIfNull(ref parameters);
            set => parameters = Bind(value);
        }

        private ScopeExpr? scope;

        public ScopeExpr Scope {
            get => InitIfNull(ref scope);
            set => scope = Bind(value);
        }

        public override TypeName? ValueType {
            get {
                try {
                    var returns =
                        Scope.FindItemsOfType<ReturnExpr>();
                    // TODO: handle all possible returns (type unions)
                    if (returns.Count > 0) {
                        return returns[0].item.ValueType;
                    }

                    return new SimpleTypeName(this, Spec.VoidType);
                }
                catch {
                    return new SimpleTypeName(this, Spec.UnknownType);
                }
            }
        }

        public FunctionDef(Node parent) : base(parent) { }

        public DecoratedExpr WithDecorators(params Expr[] items) {
            return new(Parent) {
                Target     = this,
                Decorators = new NodeList<Expr>(this, items)
            };
        }

        public FunctionDef WithScope(params Expr[] items) {
            return WithScope((IEnumerable<Expr>) items);
        }

        public FunctionDef WithScope(IEnumerable<Expr> items) {
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
