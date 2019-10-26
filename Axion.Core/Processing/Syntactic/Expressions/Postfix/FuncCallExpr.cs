using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         call_expr:
    ///             atom '(' [arg_list | (arg comprehension)] ')';
    ///     </c>
    /// </summary>
    public class FuncCallExpr : Expr {
        private Expr target;

        public Expr Target {
            get => target;
            set => SetNode(ref target, value);
        }

        private NodeList<FuncCallArg> args;

        public NodeList<FuncCallArg> Args {
            get => args;
            set => SetNode(ref args, value);
        }

        public FuncCallExpr(
            Expr                 parent = null,
            Expr                 target = null,
            params FuncCallArg[] args
        ) : base(parent) {
            Target = target;
            Args   = new NodeList<FuncCallArg>(this, args);
        }

        public FuncCallExpr Parse(bool allowGenerator = false) {
            SetSpan(() => {
                Stream.Eat(OpenParenthesis);
                Args = allowGenerator
                    ? FuncCallArg.ParseGeneratorOrArgList(this)
                    : FuncCallArg.ParseArgList(this);
                Stream.Eat(CloseParenthesis);
            });
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
            set => SetNode(ref name, value);
        }

        private Expr val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal FuncCallArg(Expr parent = null, Expr value = null) : base(parent) {
            MarkPosition(Value = value);
        }

        internal FuncCallArg(
            Expr     parent = null,
            NameExpr name   = null,
            Expr     value  = null
        ) : base(parent) {
            MarkStart(Name = name);
            MarkEnd(Value  = value);
        }

        /// <summary>
        ///     <c>
        ///         arg_list:
        ///             { expr
        ///             | expr '=' expr
        ///             | comprehension };
        ///     </c>
        /// </summary>
        internal static NodeList<FuncCallArg> ParseGeneratorOrArgList(Expr parent) {
            if (parent.Stream.Peek.Is(CloseParenthesis, OpMultiply, OpPower)) {
                return ParseArgList(parent);
            }

            Expr        nameOrValue = Parsing.ParseInfix(parent);
            var         generator   = false;
            FuncCallArg arg;
            if (parent.Stream.MaybeEat(OpAssign)) {
                // Keyword argument
                arg = FinishNamedArg(parent, nameOrValue);
            }
            else if (parent.Stream.Peek.Is(KeywordFor)) {
                // Generator expr
                arg = new FuncCallArg(
                    parent,
                    new ForComprehension(parent, nameOrValue) {
                        IsGenerator = true
                    }.Parse()
                );
                generator = true;
            }
            else {
                arg = new FuncCallArg(parent, nameOrValue);
            }

            // Was this all 
            if (!generator && parent.Stream.MaybeEat(Comma)) {
                return ParseArgList(parent, arg);
            }

            return new NodeList<FuncCallArg>(parent) {
                arg
            };
        }

        /// <summary>
        ///     <c>
        ///         arg_list:
        ///             { argument ',' }
        ///             ( argument [',']
        ///             | '*' expr [',' '**' expr]
        ///             | '**' expr );
        ///         argument:
        ///             expr ['=' expr];
        ///     </c>
        /// </summary>
        internal static NodeList<FuncCallArg> ParseArgList(
            Expr        parent = null,
            FuncCallArg first  = null
        ) {
            var args = new NodeList<FuncCallArg>(parent);

            if (first != null) {
                args.Add(first);
            }

            if (parent.Stream.PeekIs(CloseParenthesis)) {
                return args;
            }

            while (true) {
                Expr        nameOrValue = Parsing.ParseInfix(parent);
                FuncCallArg arg;

                if (parent.Stream.MaybeEat(OpMultiply)) {
                    arg = new FuncCallArg(parent, nameOrValue);
                }
                else if (parent.Stream.MaybeEat(OpAssign)) {
                    arg = FinishNamedArg(parent, nameOrValue);
                    if (!IsUniqueNamedArg(args, arg)) {
                        LangException.Report(BlameType.DuplicatedNamedArgument, arg);
                    }
                }
                else {
                    arg = new FuncCallArg(parent, nameOrValue);
                }

                args.Add(arg);
                if (!parent.Stream.MaybeEat(Comma)) {
                    break;
                }
            }

            return args;
        }

        private static FuncCallArg FinishNamedArg(Expr parent = null, Expr nameOrValue = null) {
            if (nameOrValue is NameExpr name) {
                Expr value = Parsing.ParseInfix(parent);
                return new FuncCallArg(parent, name, value);
            }

            LangException.ReportUnexpectedSyntax(Identifier, nameOrValue);
            return new FuncCallArg(parent, nameOrValue);
        }

        private static bool IsUniqueNamedArg(IEnumerable<FuncCallArg> args, FuncCallArg arg) {
            foreach (FuncCallArg a in args) {
                if (a.Name.ToString() == arg.Name.ToString()) {
                    return false;
                }
            }

            return true;
        }

        public override void ToAxion(CodeWriter c) {
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

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }
    }
}