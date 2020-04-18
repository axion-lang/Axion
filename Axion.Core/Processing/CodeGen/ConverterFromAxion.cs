using System;
using System.Linq;
using System.Web;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.CodeGen {
    public abstract class ConverterFromAxion {
        protected CodeWriter cw { get; }

        public ConverterFromAxion(CodeWriter writer) {
            cw = writer;
        }

        public virtual void Convert(Token e) {
            cw.Write(e.Value);
        }

        public virtual void Convert(CharToken e) {
            cw.Write("'", HttpUtility.JavaScriptStringEncode(e.Content));
            if (!e.IsUnclosed) {
                cw.Write("'");
            }
        }

        public virtual void Convert(CommentToken e) {
            if (e.IsMultiline) {
                cw.Write("/*" + e.Content);
                if (!e.IsUnclosed) {
                    cw.Write("*/");
                }
            }
            else {
                cw.Write("//" + e.Content);
            }
        }

        public virtual void Convert(StringToken e) {
            if (e.HasPrefix("f")) {
                cw.Write("$");
            }

            cw.Write("\"", HttpUtility.JavaScriptStringEncode(e.Content));
            if (!e.IsUnclosed) {
                cw.Write("\"");
            }
        }

        public virtual void Convert(AwaitExpr e) {
            cw.Write("await ", e.Value);
        }

        public virtual void Convert(CodeQuoteExpr e) {
            cw.Write("{{", e.Scope, "}}");
        }

        public virtual void Convert(ConstantExpr e) {
            cw.Write(e.Literal);
        }

        public virtual void Convert(NameExpr e) {
            cw.Write(string.Join("", e.Tokens.Select(t => t.Content)));
        }

        public virtual void Convert(YieldExpr e) {
            cw.Write("yield ");
            if (e.IsYieldFrom) {
                cw.Write("from ");
            }

            cw.Write(e.Value);
        }

        public virtual void Convert(ClassDef e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(FunctionDef e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(MacroDef e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(ModuleDef e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(NameDef e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(VarDef e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(BinaryExpr e) {
            cw.Write(
                e.Left,
                " ",
                e.Operator.Value,
                " ",
                e.Right
            );
        }

        public virtual void Convert(TernaryExpr e) {
            cw.Write(e.TrueExpr, " if ", e.Condition);
            if (e.FalseExpr != null) {
                cw.Write(" else ", e.FalseExpr);
            }
        }

        public virtual void Convert(UnaryExpr e) {
            if (e.Operator.Side == InputSide.Right) {
                cw.Write(
                    e.Operator.Value,
                    " (",
                    e.Value,
                    ")"
                );
            }
            else {
                cw.Write(
                    "(",
                    e.Value,
                    ") ",
                    e.Operator.Value
                );
            }
        }

        public virtual void Convert(CodeUnquotedExpr e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(ForComprehension e) {
            if (!e.IsNested) {
                cw.Write(e.Target);
            }

            cw.Write(
                " for ",
                e.Item,
                " in ",
                e.Iterable,
                e.Right
            );
        }

        public virtual void Convert(FuncCallExpr e) {
            cw.Write(e.Target, "(");
            cw.AddJoin(", ", e.Args);
            cw.Write(")");
        }

        public virtual void Convert(FunctionParameter e) {
            cw.Write(e.Name);
            if (e.ValueType != null) {
                cw.Write(": ", e.ValueType);
            }

            if (e.Value != null) {
                cw.Write(" = ", e.Value);
            }
        }

        public virtual void Convert(FuncCallArg e) {
            if (e.Name != null) {
                cw.Write(e.Name, " = ");
            }

            cw.Write(e.Value);
        }

        public virtual void Convert(IndexerExpr e) {
            cw.Write(e.Target);
            if (e.Index is SliceExpr) {
                cw.Write(e.Index);
            }
            else {
                cw.Write("[", e.Index, "]");
            }
        }

        public virtual void Convert(MemberAccessExpr e) {
            cw.Write(e.Target, ".", e.Member);
        }

        public virtual void Convert(SliceExpr e) {
            cw.Write(
                "[",
                e.From,
                ":",
                e.To,
                ":",
                e.Step,
                "]"
            );
        }

        public virtual void Convert(BreakExpr e) {
            cw.Write("break");
        }

        public virtual void Convert(ContinueExpr e) {
            cw.Write("continue");
        }

        public virtual void Convert(EmptyExpr e) {
            cw.Write(e.Mark);
        }

        public virtual void Convert(ReturnExpr e) {
            cw.Write("return");
            if (e.Value != null) {
                cw.Write(" ", e.Value);
            }
        }

        public virtual void Convert(WhileExpr e) {
            cw.Write("while ", e.Condition, e.Scope);
            if (e.NoBreakScope != null) {
                cw.Write("nobreak", e.NoBreakScope);
            }
        }

        public virtual void Convert(ArrayTypeName e) {
            cw.Write(e.ElementType, "[]");
        }

        public virtual void Convert(FuncTypeName e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(GenericTypeName e) {
            cw.Write(e.Target, "[");
            cw.AddJoin(",", e.TypeArguments);
            cw.Write("]");
        }

        public virtual void Convert(SimpleTypeName e) {
            cw.Write(e.Name);
        }

        public virtual void Convert(TupleTypeName e) {
            cw.Write("(");
            cw.AddJoin(", ", e.Types);
            cw.Write(")");
        }

        public virtual void Convert(UnionTypeName e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(Ast e) {
            cw.AddJoin("\n", e.Items);
        }

        public virtual void Convert(DecorableExpr e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(IfExpr e) {
            cw.Write("if ", e.Condition, e.ThenScope);
            if (e.ElseScope != null) {
                cw.Write("else", e.ElseScope);
            }
        }

        public virtual void Convert(MacroApplicationExpr e) {
            cw.AddJoin(" ", e.Expressions);
        }

        public virtual void Convert(ParenthesizedExpr e) {
            cw.Write("(", e.Value, ")");
        }

        public virtual void Convert(ScopeExpr e) {
            throw new NotSupportedException();
        }

        public virtual void Convert(TupleExpr e) {
            cw.Write("(");
            cw.AddJoin(", ", e.Expressions);
            cw.Write(")");
        }
    }
}
