using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         func_call_expr:
    ///             atom '(' [arg_list | (arg for_comprehension)] ')';
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
                Args = FuncCallArg.ParseArgList(this, allowGenerator: allowGenerator);
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
            Expr        parent         = null,
            FuncCallArg first          = null,
            bool        allowGenerator = false
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
                    var name = (NameExpr) Parsing.ParseAtom(parent);
                    parent.Stream.Eat(OpAssign);
                    Expr value = Parsing.ParseInfix(parent);
                    arg = new FuncCallArg(parent, name, value);
                    if (args.Any(a => a.Name.ToString() == name.ToString())) {
                        LangException.Report(BlameType.DuplicatedNamedArgument, arg);
                    }
                }
                else {
                    Expr value = Parsing.ParseInfix(parent);
                    // generator arg
                    if (parent.Stream.PeekIs(KeywordFor)) {
                        arg = new FuncCallArg(
                            parent,
                            new ForComprehension(parent, value) {
                                IsGenerator = true
                            }.Parse()
                        );
                    }
                    else {
                        // TODO: star args
                        parent.Stream.MaybeEat(OpMultiply);
                        arg = new FuncCallArg(parent, value);
                    }
                }

                args.Add(arg);
                if (!parent.Stream.MaybeEat(Comma)) {
                    break;
                }
            }

            return args;
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