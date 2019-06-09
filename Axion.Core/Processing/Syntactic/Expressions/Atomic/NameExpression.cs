using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         name:
    ///             simple_name | qualified_name
    ///     </c>
    /// </summary>
    public abstract class NameExpression : Expression {
        public abstract string Name { get; }
        protected NameExpression() { }
        protected NameExpression(AstNode parent) : base(parent) { }

        internal static NameExpression ParseName(AstNode parent) {
            var qualifiers = new List<Token>();
            do {
                parent.Eat(TokenType.Identifier);
                qualifiers.Add(parent.Token);
            } while (parent.MaybeEat(TokenType.Dot));

            if (qualifiers.Count > 1) {
                return new QualifiedNameExpression(parent, qualifiers.Select(q => q.Value));
            }

            return new SimpleNameExpression(parent, qualifiers[0]);
        }

        internal static NameExpression ParseName(AstNode parent, string name) {
            string[] qualifiers = name.Split('.');
            if (qualifiers.Length > 1) {
                return new QualifiedNameExpression(parent, qualifiers);
            }

            return new SimpleNameExpression(parent, qualifiers[0]);
        }
    }
}