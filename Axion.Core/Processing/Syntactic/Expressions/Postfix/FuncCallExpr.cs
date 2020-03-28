using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         func-call-expr:
    ///             atom '(' [multiple-arg | (arg for-comprehension)] ')';
    ///     </c>
    /// </summary>
    public class FuncCallExpr : PostfixExpr {
        private Expr target;

        public Expr Target {
            get => target;
            set => target = Bind(value);
        }

        private NodeList<FuncCallArg> args;

        public NodeList<FuncCallArg> Args {
            get => args;
            set => args = Bind(value);
        }

        public FuncCallExpr(
            Expr?                parent = null,
            Expr?                target = null,
            params FuncCallArg[] args
        ) : base(
            parent
         ?? GetParentFromChildren(target)
        ) {
            Target = target;
            Args   = NodeList<FuncCallArg>.From(this, args);
        }

        public FuncCallExpr Parse(bool allowGenerator = false) {
            SetSpan(
                () => {
                    Stream.Eat(OpenParenthesis);
                    Args = FuncCallArg.ParseArgList(this, allowGenerator: allowGenerator);
                    Stream.Eat(CloseParenthesis);
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Target, "(");
            c.AddJoin(", ", Args);
            c.Write(")");
        }

        public override void ToCSharp(CodeWriter c) {
            ToAxion(c);
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }

        public override void ToPascal(CodeWriter c) {
            ToAxion(c);
        }
    }

    public sealed class FuncCallArg : Expr {
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => name = Bind(value);
        }

        private Expr val;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        internal FuncCallArg(
            Expr?     parent = null,
            NameExpr? name   = null,
            Expr?     value  = null
        ) : base(
            parent
         ?? GetParentFromChildren(name, value)
        ) {
            Name  = name;
            Value = value;
            MarkStart(Name);
            MarkEnd(Value);
        }

        /// <summary>
        ///     <c>
        ///         multiple-arg:
        ///             comprehension
        ///             | ({ argument ',' }
        ///                (argument [',']
        ///                | '*' expr [',' '**' expr]
        ///                | '**' expr ));
        ///         argument:
        ///             expr ['=' expr];
        ///     </c>
        /// </summary>
        internal static NodeList<FuncCallArg> ParseArgList(
            Expr         parent,
            FuncCallArg? first          = null,
            bool         allowGenerator = false
        ) {
            var args = new NodeList<FuncCallArg>(parent);

            if (first != null) {
                args.Add(first);
            }

            if (parent.Stream.PeekIs(CloseParenthesis)) {
                return args;
            }

            while (true) {
                FuncCallArg arg;
                // named arg
                if (parent.Stream.PeekIs(Identifier) && parent.Stream.PeekByIs(2, OpAssign)) {
                    var argName = (NameExpr) AtomExpr.Parse(parent);
                    parent.Stream.Eat(OpAssign);
                    InfixExpr argValue = InfixExpr.Parse(parent);
                    arg = new FuncCallArg(parent, argName, argValue);
                    if (args.Any(a => a.Name.ToString() == argName.ToString())) {
                        LangException.Report(BlameType.DuplicatedNamedArgument, arg);
                    }
                }
                else {
                    Expr argValue = InfixExpr.Parse(parent);
                    // generator arg
                    if (parent.Stream.PeekIs(KeywordFor)) {
                        arg = new FuncCallArg(
                            parent,
                            value: new ForComprehension(parent, argValue) { IsGenerator = true }.Parse()
                        );
                    }
                    else {
                        // TODO: star args
                        parent.Stream.MaybeEat(OpMultiply);
                        arg = new FuncCallArg(parent, value: argValue);
                    }
                }

                args.Add(arg);
                if (!parent.Stream.MaybeEat(Comma)) {
                    break;
                }
            }

            return args;
        }

        public override void ToDefault(CodeWriter c) {
            if (Name != null) {
                c.Write(Name, " = ");
            }

            c.Write(Value);
        }

        public override void ToCSharp(CodeWriter c) {
            if (Name != null) {
                c.Write(Name, ": ");
            }

            c.Write(Value);
        }
    }
}