using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Parser {
    public partial class SyntaxParser {
        /// <summary>
        ///     <c>
        ///         arg_list ::=
        ///             { expr
        ///             | expr '=' expr
        ///             | expr 'for' }
        ///     </c>
        /// </summary>
        private Arg[] FinishGeneratorOrArgList() {
            if (stream.PeekIs(
                TokenType.RightParenthesis,
                TokenType.OpMultiply,
                TokenType.OpPower
            )) {
                return ParseArgumentsList();
            }
            Position   start          = tokenStart;
            Expression argNameOrValue = ParseTestExpr();
            if (argNameOrValue is ErrorExpression) {
                return new Arg[0];
            }
            var generator = false;
            Arg arg;
            if (stream.MaybeEat(TokenType.Assign)) {
                // Keyword argument
                arg = FinishKeywordArgument(argNameOrValue);
            }
            else if (stream.PeekIs(TokenType.KeywordFor)) {
                // Generator expr
                arg       = new Arg(ParseGeneratorExpr(argNameOrValue));
                generator = true;
            }
            else {
                arg = new Arg(argNameOrValue);
            }

            // Was this all?
            if (!generator && stream.MaybeEat(TokenType.Comma)) {
                return ParseArgumentsList(arg);
            }

            stream.Eat(TokenType.RightParenthesis);
            arg.MarkPosition(start, tokenEnd);
            return new[] { arg };
        }

        private Arg FinishKeywordArgument(Expression expr) {
            if (expr is NameExpression name) {
                Expression value = ParseTestExpr();
                return new Arg(name, value);
            }
            BlameInvalidSyntax(TokenType.Identifier, expr);
            return new Arg(expr);
        }

        private void CheckUniqueArgument(List<Arg> arguments, Arg arg) {
            if (arg?.Name.Name == null) {
                return;
            }
            foreach (Arg a in arguments) {
                if (a.Name.Name == arg.Name.Name) {
                    unit.Blame(BlameType.DuplicatedKeywordArgument, arg);
                }
            }
        }

        /// <summary>
        ///     <c>
        ///         arg_list ::=
        ///             { argument ',' }
        ///             ( argument [',']
        ///             | '*' expr [',' '**' expr]
        ///             | '**' expr )
        ///         argument ::=
        ///             expr ['=' expr]
        ///     </c>
        /// </summary>
        private Arg[] ParseArgumentsList(Arg first = null) {
            var arguments = new List<Arg>();

            if (first != null) {
                arguments.Add(first);
            }

            while (!stream.MaybeEat(TokenType.RightParenthesis)) {
                Expression nameOrValue = ParseTestExpr();
                Arg        arg;

                if (stream.MaybeEat(TokenType.OpMultiply)) {
                    arg = new Arg(nameOrValue);
                }
                else {
                    if (stream.MaybeEat(TokenType.Assign)) {
                        arg = FinishKeywordArgument(nameOrValue);
                        CheckUniqueArgument(arguments, arg);
                    }
                    else {
                        arg = new Arg(nameOrValue);
                    }
                }
                arguments.Add(arg);
                if (!stream.MaybeEat(TokenType.Comma)) {
                    stream.Eat(TokenType.RightParenthesis);
                    break;
                }
            }

            return arguments.ToArray();
        }
    }
}