using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Atomic;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     <c>
    ///         call_expr:
    ///             atom '(' [arg_list | (arg comprehension)] ')'
    ///     </c>
    /// </summary>
    public class FunctionCallExpression : Expression {
        private Expression target;

        public Expression Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private NodeList<CallArgument> args;

        public NodeList<CallArgument> Args {
            get => args;
            set => SetNode(ref args, value);
        }

        /// <summary>
        ///     Constructor for pipeline operator.
        /// </summary>
        public FunctionCallExpression(
            Expression            parent,
            Expression            target,
            params CallArgument[] args
        ) : base(parent) {
            Target = target;
            Args   = new NodeList<CallArgument>(this, args);
        }

        public FunctionCallExpression(
            Expression parent,
            Expression target,
            bool       allowGenerator = false
        ) {
            Construct(parent, target, () => {
                Target = target;
                Eat(OpenParenthesis);
                Args = allowGenerator
                    ? CallArgument.ParseGeneratorOrArgList(this)
                    : CallArgument.ParseArgList(this);
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Target, "(");
            c.AddJoin(", ", Args);
            c.Write(")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Target, "(");
            c.AddJoin(", ", Args);
            c.Write(")");
        }
    }

    public enum ArgumentKind {
        Simple,
        Named,
        List,
        Map
    }

    public sealed class CallArgument : Expression {
        private SimpleNameExpression name;

        public SimpleNameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal CallArgument(Expression parent, Expression value) : base(parent) {
            MarkPosition(Value = value);
        }

        internal CallArgument(
            Expression           parent,
            SimpleNameExpression name,
            Expression           value
        ) : base(parent) {
            MarkStart(Name = name);
            MarkEnd(Value  = value);
        }

        /// <summary>
        ///     <c>
        ///         arg_list:
        ///             { expr
        ///             | expr '=' expr
        ///             | comprehension }
        ///     </c>
        /// </summary>
        internal static NodeList<CallArgument> ParseGeneratorOrArgList(Expression parent) {
            if (parent.Peek.Is(CloseParenthesis, OpMultiply, OpPower)) {
                return ParseArgList(parent);
            }

            Expression   nameOrValue = ParseInfixExpr(parent);
            var          generator   = false;
            CallArgument arg;
            if (parent.MaybeEat(OpAssign)) {
                // Keyword argument
                arg = FinishNamedArg(parent, nameOrValue);
            }
            else if (parent.Peek.Is(KeywordFor)) {
                // Generator expr
                arg = new CallArgument(
                    parent,
                    new GeneratorExpression(parent, new ForComprehension(parent, nameOrValue))
                );
                generator = true;
            }
            else {
                arg = new CallArgument(parent, nameOrValue);
            }

            // Was this all 
            if (!generator && parent.MaybeEat(Comma)) {
                return ParseArgList(parent, arg);
            }

            parent.Eat(CloseParenthesis);
            return new NodeList<CallArgument>(parent) {
                arg
            };
        }

        /// <summary>
        ///     <c>
        ///         arg_list:
        ///             { argument ',' }
        ///             ( argument [',']
        ///             | '*' expr [',' '**' expr]
        ///             | '**' expr )
        ///         argument:
        ///             expr ['=' expr]
        ///     </c>
        /// </summary>
        internal static NodeList<CallArgument> ParseArgList(
            Expression   parent,
            CallArgument first = null
        ) {
            var args = new NodeList<CallArgument>(parent);

            if (first != null) {
                args.Add(first);
            }

            while (!parent.MaybeEat(CloseParenthesis)) {
                Expression   nameOrValue = ParseInfixExpr(parent);
                CallArgument arg;

                if (parent.MaybeEat(OpMultiply)) {
                    arg = new CallArgument(parent, nameOrValue);
                }
                else if (parent.MaybeEat(OpAssign)) {
                    arg = FinishNamedArg(parent, nameOrValue);
                    if (!IsUniqueNamedArg(args, arg)) {
                        parent.Unit.Blame(BlameType.DuplicatedNamedArgument, arg);
                    }
                }
                else {
                    arg = new CallArgument(parent, nameOrValue);
                }

                args.Add(arg);
                if (parent.MaybeEat(Comma)) {
                    continue;
                }

                parent.Eat(CloseParenthesis);
                break;
            }

            return args;
        }

        private static CallArgument FinishNamedArg(Expression parent, Expression nameOrValue) {
            if (nameOrValue is SimpleNameExpression name) {
                Expression value = ParseInfixExpr(parent);
                return new CallArgument(parent, name, value);
            }

            parent.BlameInvalidSyntax(Identifier, nameOrValue);
            return new CallArgument(parent, nameOrValue);
        }

        private static bool IsUniqueNamedArg(IEnumerable<CallArgument> args, CallArgument arg) {
            foreach (CallArgument a in args) {
                if (a.Name.Name == arg.Name.Name) {
                    return false;
                }
            }

            return true;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            if (Name != null) {
                c.Write(Name + " = ");
            }

            c.Write(Value);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            if (Name != null) {
                c.Write(Name + " = ");
            }

            c.Write(Value);
        }
    }
}