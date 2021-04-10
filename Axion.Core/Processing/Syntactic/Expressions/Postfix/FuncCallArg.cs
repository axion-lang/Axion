using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;
using Axion.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    [SyntaxExpression]
    public partial class FuncCallArg : Node {
        [LeafSyntaxNode] NameExpr? name;
        [LeafSyntaxNode] Node value = null!;

        internal FuncCallArg(Node parent) : base(parent) { }

        /// <summary>
        ///     <code>
        ///         multiple-arg:
        ///             comprehension
        ///             | ({ argument ',' }
        ///                (argument [',']
        ///                | '*' expr [',' '**' expr]
        ///                | '**' expr ));
        ///         argument:
        ///             expr ['=' expr];
        ///     </code>
        /// </summary>
        internal static NodeList<FuncCallArg> ParseArgList(
            Node         parent,
            FuncCallArg? first          = null,
            bool         allowGenerator = false
        ) {
            var s = parent.Unit.TokenStream;
            var args = new NodeList<FuncCallArg>(parent);

            if (first != null) {
                args += first;
            }

            if (s.PeekIs(TokenType.CloseParenthesis)) {
                return args;
            }

            while (true) {
                FuncCallArg arg;
                // named arg
                if (s.PeekIs(TokenType.Identifier)
                 && s.PeekByIs(2, TokenType.EqualsSign)) {
                    var argName = (NameExpr) AtomExpr.Parse(parent);
                    s.Eat(TokenType.EqualsSign);
                    var argValue = InfixExpr.Parse(parent);
                    arg = new FuncCallArg(parent) {
                        Name  = argName,
                        Value = argValue
                    };
                    if (args.Any(
                        a => a.Name?.ToString() == argName.ToString()
                    )) {
                        LanguageReport.To(
                            BlameType.DuplicatedNamedArgument,
                            arg
                        );
                    }
                }
                else {
                    Node argValue = InfixExpr.Parse(parent);
                    // generator arg
                    if (s.PeekIs(TokenType.KeywordFor)) {
                        arg = new FuncCallArg(parent) {
                            Value = new ForComprehension(parent) {
                                Target = argValue
                            }.Parse()
                        };
                    }
                    else {
                        // TODO: star args
                        s.MaybeEat(TokenType.Star);
                        arg = new FuncCallArg(parent) {
                            Value = argValue
                        };
                    }
                }

                args += arg;
                if (!s.MaybeEat(TokenType.Comma)) {
                    break;
                }
            }

            return args;
        }
    }
}
