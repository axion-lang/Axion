using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         name-expr:
    ///             ID {'.' ID};
    ///     </c>
    /// </summary>
    public class NameExpr : AtomExpr {
        public List<Token> Tokens     { get; }
        public List<Token> Qualifiers => Tokens.Where(t => t.Type == Identifier).ToList();
        public bool        IsSimple   => Qualifiers.Count == 1;

        [NoTraversePath]
        public override TypeName ValueType =>
            ((Expr) GetParentOfType<ScopeExpr>().GetDefByName(ToString()))?.ValueType;

        public NameExpr(string name) {
            Tokens = new List<Token>();
            if (name.Contains('.')) {
                string[] qs = name.Split('.');
                for (var i = 0; i < qs.Length; i++) {
                    string q = qs[i];
                    Tokens.Add(new Token(Source, Identifier, q));
                    if (i != qs.Length - 1) {
                        Tokens.Add(new Token(Source, OpDot, "."));
                    }
                }
            }
            else {
                Tokens.Add(new Token(Source, Identifier, name));
            }
        }

        public NameExpr(Expr parent, IEnumerable<Token>? tokens = null) : base(parent) {
            Tokens = tokens?.ToList() ?? new List<Token>();
        }

        public NameExpr Parse(bool simple = false) {
            SetSpan(
                () => {
                    Stream.Eat(Identifier);
                    Tokens.Add(Stream.Token);
                    if (!simple) {
                        while (Stream.MaybeEat(OpDot)) {
                            Tokens.Add(Stream.Token);
                            if (Stream.Eat(Identifier) != null) {
                                Tokens.Add(Stream.Token);
                            }
                        }
                    }
                }
            );
            return this;
        }

        public override string ToString() {
            return string.Join("", Tokens.Select(t => t.Content));
        }

        public override void ToAxion(CodeWriter c) {
            c.AddJoin(".", Qualifiers);
        }

        public override void ToCSharp(CodeWriter c) {
            if (IsSimple && Qualifiers[0].Content == "self") {
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