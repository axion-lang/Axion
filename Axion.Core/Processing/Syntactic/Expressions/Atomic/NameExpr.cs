using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         name-expr:
    ///             ID {'.' ID};
    ///     </c>
    /// </summary>
    public class NameExpr : AtomExpr {
        private NodeList<Token>? tokens;

        public NodeList<Token> Tokens {
            get => InitIfNull(ref tokens);
            set => tokens = Bind(value);
        }

        public Token[] Qualifiers => Tokens.OfType(Identifier);
        public bool    IsSimple   => Qualifiers.Length == 1;

        public override TypeName? ValueType =>
            ((Expr?) GetParent<ScopeExpr>()?.GetDefByName(this))?.ValueType;

        public NameExpr(Node parent) : base(parent) { }

        public NameExpr(Node parent, string name) : base(parent) {
            if (name.Contains('.')) {
                string[] qs = name.Split('.');
                for (var i = 0; i < qs.Length; i++) {
                    string q = qs[i];
                    Tokens.Add(new Token(Unit, Identifier, q));
                    if (i != qs.Length - 1) {
                        Tokens.Add(new Token(Unit, OpDot, "."));
                    }
                }
            }
            else {
                Tokens.Add(new Token(Unit, Identifier, name));
            }
        }

        public NameExpr Parse(bool simple = false) {
            Tokens.Add(Stream.Eat(Identifier));
            if (simple) {
                return this;
            }

            while (Stream.PeekIs(OpDot)) {
                Tokens.Add(Stream.Eat());
                Tokens.Add(Stream.Eat(Identifier));
            }

            return this;
        }

        public override string ToString() {
            return string.Join(".", Qualifiers.Select(q => q.Content));
        }
    }
}
