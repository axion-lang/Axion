using System;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Translation;

namespace Axion.Frontend.Axion {
    public class Converter : INodeConverter {
        public string OutputFileExtension => ".ax";

        public bool Convert(CodeWriter w, IConvertibleNode node) {
            switch (node) {
            case Token e: {
                // NOTE: all saved token.Value's are valid Axion code.
                w.Write(e.Value, e.EndingWhite);
                break;
            }
            case AwaitExpr e: {
                w.Write("await ", e.Value);
                break;
            }
            case CodeQuoteExpr e: {
                w.Write("{{ ", e.Scope, "}}");
                break;
            }
            case ConstantExpr e: {
                w.Write(e.Literal);
                break;
            }
            case NameExpr e: {
                w.Write(string.Join("", e.Tokens.Select(t => t.Value)));
                break;
            }
            case YieldExpr e: {
                w.Write("yield ");
                if (e.KwFrom != null)
                    w.Write("from ");

                w.Write(e.Value);
                break;
            }
            case ClassDef _: {
                throw new NotSupportedException();
            }
            case FunctionDef e: {
                w.Write("fn ");
                if (e.Name != null) {
                    w.Write(e.Name, " ");
                }

                if (e.Parameters.Count > 0) {
                    w.Write("(");
                    w.AddJoin(", ", e.Parameters);
                    w.Write(") ");
                }

                if (e.ValueType != null) {
                    w.Write("-> ", e.ValueType);
                }

                w.Write(e.Scope);
                break;
            }
            case MacroDef e: {
                w.Write(
                    "macro ",
                    e.Name,
                    "(",
                    e.Syntax,
                    ")",
                    e.Scope
                );
                break;
            }
            case ModuleDef e: {
                w.Write("module ", e.Name, e.Scope);
                break;
            }
            case FunctionParameter e: {
                w.Write(e.Name);
                if (e.ValueType != null)
                    w.Write(": ", e.ValueType);

                if (e.Value != null)
                    w.Write(" = ", e.Value);

                break;
            }
            case VarDef e: {
                if (e.IsImmutable) {
                    w.Write("let ");
                }

                w.Write(e.Name);
                if (e.ValueType != null) {
                    w.Write(": ", e.ValueType);
                }

                if (e.Value != null) {
                    w.Write(" = ", e.Value);
                }

                break;
            }
            case NameDef e: {
                w.Write(e.Name);
                if (e.ValueType != null) {
                    w.Write(": ", e.ValueType);
                }

                if (e.Value != null) {
                    w.Write(" = ", e.Value);
                }

                break;
            }
            case BinaryExpr e: {
                w.Write(e.Left, e.Operator!.Value, e.Right);
                break;
            }
            case TernaryExpr e: {
                w.Write(e.TrueExpr, " if ", e.Condition);
                if (e.FalseExpr != null)
                    w.Write(" else ", e.FalseExpr);

                break;
            }
            case UnaryExpr e: {
                if (e.Operator.Side == InputSide.Right)
                    w.Write(e.Operator.Value, e.Value);
                else
                    w.Write(e.Value, e.Operator.Value);

                break;
            }
            case CodeUnquotedExpr e: {
                w.Write("$", e.Value);
                break;
            }
            case ForComprehension e: {
                if (!e.IsNested)
                    w.Write(e.Target);

                w.Write(
                    " for ",
                    e.Item,
                    " in ",
                    e.Iterable,
                    e.Right
                );
                break;
            }
            case FuncCallExpr e: {
                w.Write(e.Target, "(");
                w.AddJoin(", ", e.Args);
                w.Write(")");
                break;
            }
            case FuncCallArg e: {
                if (e.Name != null)
                    w.Write(e.Name, " = ");

                w.Write(e.Value);
                break;
            }
            case IndexerExpr e: {
                w.Write(e.Target);
                if (e.Index is SliceExpr)
                    w.Write(e.Index);
                else
                    w.Write("[", e.Index, "]");

                break;
            }
            case MemberAccessExpr e: {
                w.Write(e.Target, ".", e.Member);
                break;
            }
            case SliceExpr e: {
                w.Write(
                    "[",
                    e.From,
                    ":",
                    e.To,
                    ":",
                    e.Step,
                    "]"
                );
                break;
            }
            case BreakExpr e: {
                w.Write("break");
                if (e.LoopName != null) {
                    w.Write(" ", e.LoopName);
                }

                break;
            }
            case ContinueExpr e: {
                w.Write("continue");
                if (e.LoopName != null) {
                    w.Write(" ", e.LoopName);
                }

                break;
            }
            case EmptyExpr e: {
                w.Write(e.Mark);
                break;
            }
            case ReturnExpr e: {
                w.Write("return");
                if (e.Value != null)
                    w.Write(" ", e.Value);

                break;
            }
            case WhileExpr e: {
                w.Write("while ", e.Condition, e.Scope);
                if (e.NoBreakScope != null)
                    w.Write("nobreak", e.NoBreakScope);

                break;
            }
            case ArrayTypeName e: {
                w.Write(e.ElementType, "[]");
                break;
            }
            case GenericTypeName e: {
                w.Write(e.Target, "[");
                w.AddJoin(",", e.TypeArgs);
                w.Write("]");
                break;
            }
            case SimpleTypeName e: {
                w.Write(e.Name);
                break;
            }
            case TupleTypeName e: {
                w.Write("(");
                w.AddJoin(", ", e.Types);
                w.Write(")");
                break;
            }
            case FuncTypeName e: {
                w.Write(e.ArgsType, " -> ", e.ReturnType);
                break;
            }
            case UnionTypeName e: {
                w.Write(e.Left, " | ", e.Right);
                break;
            }
            case Ast e: {
                w.AddJoin("\n", e.Items);
                break;
            }
            case DecoratedExpr e: {
                if (e.Decorators.Count == 1) {
                    w.Write("@", e.Decorators[0], e.Target);
                }
                else {
                    w.Write("@[");
                    w.AddJoin(", ", e.Decorators);
                    w.WriteLine("]");
                    w.Write(e.Target);
                }

                break;
            }
            case IfExpr e: {
                w.Write(e.BranchKw, e.Condition, e.ThenScope);
                if (e.ElseScope != null) {
                    if (e.ElseScope.Items.Count == 1
                     && e.ElseScope.Items[0] is IfExpr elif) {
                        w.Write(elif);
                    }
                    else {
                        w.Write("else", e.ElseScope);
                    }
                }

                break;
            }
            case MacroApplicationExpr e: {
                w.AddJoin(" ", e.Expressions);
                break;
            }
            case ParenthesizedExpr e: {
                w.Write("(", e.Value, ")");
                break;
            }
            case ScopeExpr e: {
                var inAnonFn = e.Parent is FunctionDef fn && fn.Name == null;
                if (inAnonFn) {
                    w.WriteLine(" {");
                }

                w.IndentLevel++;
                w.MaybeWriteLine();
                w.AddJoin("", e.Items, true);
                w.MaybeWriteLine();
                w.IndentLevel--;

                if (inAnonFn) {
                    w.Write("}");
                }

                break;
            }
            case TupleExpr e: {
                w.Write("(");
                w.AddJoin(", ", e.Expressions);
                w.Write(")");
                break;
            }
            default: {
                return false;
            }
            }

            return true;
        }
    }
}
