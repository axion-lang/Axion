using System.Web;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Statements;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Translation;

namespace Axion.Emitter.Python {
    public class Translator : INodeTranslator {
        public string OutputFileExtension => ".py";

        public bool Translate(CodeWriter w, ITranslatableNode node) {
            switch (node) {
            case CommentToken e: {
                w.Write("#", e.Content);
                break;
            }
            case StringToken e: {
                if (e.HasPrefix("f")) {
                    w.Write("f");
                }

                if (e.HasPrefix("r")) {
                    w.Write("r");
                }

                w.Write(e.IsMultiline ? "\"\"\"" : "\"");
                w.Write(HttpUtility.JavaScriptStringEncode(e.Content));
                if (!e.IsUnclosed) {
                    w.Write(e.IsMultiline ? "\"\"\"" : "\"");
                }

                break;
            }
            case ClassDef e: {
                w.Write("class ", e.Name);
                if (e.Bases.Count > 0) {
                    w.Write("(");
                    w.AddJoin(", ", e.Bases);
                    w.Write(")");
                }

                w.Write(e.Scope);
                break;
            }
            case FunctionDef e: {
                w.Write("def ", e.Name, "(");
                w.AddJoin(", ", e.Parameters);
                w.Write(")");
                if (e.ValueType != null) {
                    w.Write(" -> ", e.ValueType);
                }

                w.Write(e.Scope);
                break;
            }
            case ModuleDef e: {
                w.AddJoin("", e.Scope.Items, true);
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
            case EmptyExpr _: {
                w.Write("pass");
                break;
            }
            case WhileExpr e: {
                w.Write("while ", e.Condition, e.Scope);
                break;
            }
            case ArrayTypeName e: {
                w.Write("List[", e.ElementType, "]");
                break;
            }
            case FuncTypeName e: {
                w.Write(
                    "Callable[[",
                    e.ArgsType,
                    "], ",
                    e.ReturnType,
                    "]"
                );
                break;
            }
            case DecoratedExpr e: {
                foreach (var decorator in e.Decorators) {
                    w.WriteLine("@", decorator);
                }

                w.Write(e.Target);
                break;
            }
            case Ast e: {
                w.AddJoin("", e.Items, true);
                break;
            }
            case ScopeExpr e: {
                w.Write(":");
                w.IndentLevel++;
                w.WriteLine();
                w.AddJoin("", e.Items, true);
                w.IndentLevel--;
                w.MaybeWriteLine();
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
