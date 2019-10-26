using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         name_expr:
    ///             ID {'.' ID};
    ///     </c>
    /// </summary>
    public class NameExpr : Expr, IVarTargetExpr {
        public List<Token> Tokens { get; }

        public List<Token> Qualifiers => Tokens.Where(t => t.Type == Identifier).ToList();

        public bool IsSimple => Qualifiers.Count == 1;

        public override TypeName ValueType => ((Expr) GetParentOfType<BlockExpr>().GetDefByName(ToString()))?.ValueType;

        public NameExpr(string name) {
            Tokens = new List<Token>();
            string[] qs = name.Split('.');
            for (var i = 0; i < qs.Length; i++) {
                string q = qs[i];
                Tokens.Add(new Token(Source, Identifier, q));
                if (i != qs.Length - 1) {
                    Tokens.Add(new Token(Source, OpDot, "."));
                }
            }
        }

        public NameExpr(Expr parent = null, IEnumerable<Token> tokens = null) : base(parent) {
            Tokens = tokens?.ToList() ?? new List<Token>();
        }

        public NameExpr Parse(bool mustBeSimple = false) {
            SetSpan(() => {
                Stream.Eat(Identifier);
                Tokens.Add(Stream.Token);
                if (!mustBeSimple) {
                    while (Stream.MaybeEat(OpDot)) {
                        Tokens.Add(Stream.Token);
                        if (Stream.Eat(Identifier) != null) {
                            Tokens.Add(Stream.Token);
                        }
                    }
                }
            });
            return this;
        }

        public override string ToString() {
            return string.Join("", Tokens.Select(t => t.Content));
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(ToString());
        }

        public override void ToCSharp(CodeWriter c) {
            if (Tokens.Count == 1 && Tokens[0].Content == "self") {
                c.Write("this");
                return;
            }

            c.Write(ToString());
        }

        public override void ToPython(CodeWriter c) {
            c.Write(ToString());
        }
    }
}