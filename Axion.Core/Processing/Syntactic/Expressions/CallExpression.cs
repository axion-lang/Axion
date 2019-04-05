using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class CallExpression : Expression {
        private Expression target;

        [NotNull]
        public Expression Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private NodeList<Arg> args;

        [NotNull]
        public NodeList<Arg> Args {
            get => args;
            set => SetNode(ref args, value);
        }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        /// <summary>
        ///     Constructor for pipeline operator.
        /// </summary>
        public CallExpression(
            [NotNull] SyntaxTreeNode parent,
            [NotNull] Expression     target,
            params    Arg[]          args
        ) {
            Parent = parent;
            Target = target;
            Args   = new NodeList<Arg>(this, args);
        }

        public CallExpression(
            [NotNull] SyntaxTreeNode parent,
            [NotNull] Expression     target,
            bool                     allowGenerator = false
        ) {
            Parent = parent;
            Target = target;
            Eat(TokenType.OpenParenthesis);
            Args = allowGenerator
                ? Arg.ParseGeneratorOrArgList(this)
                : Arg.ParseArgList(this);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + Target + "(";
            c.AppendJoin(",", Args);
            return c + ")";
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + Target + "(";
            c.AppendJoin(",", Args);
            return c + ")";
        }
    }

    public enum ArgumentKind {
        Simple,
        Named,
        List,
        Map
    }

    public sealed class Arg : Expression {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal Arg(Expression value) {
            Value = value;

            MarkPosition(value);
        }

        internal Arg(NameExpression name, Expression value) {
            Name  = name;
            Value = value;

            MarkPosition(name, value);
        }

        internal ArgumentKind GetArgumentInfo() {
            if (Name == null) {
                return ArgumentKind.Simple;
            }

            if (Name.Qualifiers[0].Value == "*") {
                return ArgumentKind.List;
            }

            if (Name.Qualifiers[0].Value == "**") {
                return ArgumentKind.Map;
            }

            return ArgumentKind.Named;
        }

        /// <summary>
        ///     <c>
        ///         arg_list ::=
        ///             { expr
        ///             | expr '=' expr
        ///             | comprehension }
        ///     </c>
        /// </summary>
        internal static NodeList<Arg> ParseGeneratorOrArgList(SyntaxTreeNode parent) {
            if (parent.PeekIs(
                TokenType.CloseParenthesis,
                TokenType.OpMultiply,
                TokenType.OpPower
            )) {
                return ParseArgList(parent);
            }

            Token      start          = parent.Token;
            Expression argNameOrValue = ParseTestExpr(parent);
            if (argNameOrValue is ErrorExpression) {
                return null;
            }

            var generator = false;
            Arg arg;
            if (parent.MaybeEat(TokenType.OpAssign)) {
                // Keyword argument
                arg = FinishKeywordArgument(parent, argNameOrValue);
            }
            else if (parent.PeekIs(TokenType.KeywordFor)) {
                // Generator expr
                arg       = new Arg(new GeneratorExpression(parent, argNameOrValue));
                generator = true;
            }
            else {
                arg = new Arg(argNameOrValue);
            }

            // Was this all?
            if (!generator
                && parent.MaybeEat(TokenType.Comma)) {
                return ParseArgList(parent, arg);
            }

            parent.Eat(TokenType.CloseParenthesis);
            arg.MarkPosition(start, parent.Token);
            return new NodeList<Arg>(parent) {
                arg
            };
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
        internal static NodeList<Arg> ParseArgList(SyntaxTreeNode parent, Arg first = null) {
            var arguments = new NodeList<Arg>(parent);

            if (first != null) {
                arguments.Add(first);
            }

            while (!parent.MaybeEat(TokenType.CloseParenthesis)) {
                Expression nameOrValue = ParseTestExpr(parent);
                Arg        arg;

                if (parent.MaybeEat(TokenType.OpMultiply)) {
                    arg = new Arg(nameOrValue);
                }
                else {
                    if (parent.MaybeEat(TokenType.OpAssign)) {
                        arg = FinishKeywordArgument(parent, nameOrValue);
                        CheckUniqueArgument(parent, arguments, arg);
                    }
                    else {
                        arg = new Arg(nameOrValue);
                    }
                }

                arguments.Add(arg);
                if (parent.MaybeEat(TokenType.Comma)) {
                    continue;
                }

                parent.Eat(TokenType.CloseParenthesis);
                break;
            }

            return arguments;
        }

        private static Arg FinishKeywordArgument(SyntaxTreeNode parent, Expression expr) {
            if (expr is NameExpression name) {
                Expression value = ParseTestExpr(parent);
                return new Arg(name, value);
            }

            parent.BlameInvalidSyntax(TokenType.Identifier, expr);
            return new Arg(expr);
        }

        private static void CheckUniqueArgument(
            SyntaxTreeNode   parent,
            IEnumerable<Arg> arguments,
            Arg              arg
        ) {
            if (arg.Name.Qualifiers.Count == 0) {
                return;
            }

            foreach (Arg a in arguments) {
                if (a.Name.Qualifiers == arg.Name.Qualifiers) {
                    parent.Unit.Blame(BlameType.DuplicatedKeywordArgument, arg);
                }
            }
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            if (Name != null) {
                c = c + Name + " = ";
            }

            return c + Value;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            if (Name != null) {
                c = c + Name + "=";
            }

            return c + Value;
        }
    }
}