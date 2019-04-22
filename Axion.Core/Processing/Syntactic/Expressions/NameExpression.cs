using System.Collections.Generic;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         name:
    ///             ID {'.' ID}
    ///     </c>
    /// </summary>
    public abstract class NameExpression : Expression {
        protected NameExpression() { }
        protected NameExpression(SyntaxTreeNode parent) : base(parent) { }

        internal static NameExpression ParseName(SyntaxTreeNode parent) {
            var qualifiers = new List<string>();
            do {
                parent.Eat(TokenType.Identifier);
                qualifiers.Add(parent.Token.Value);
            } while (parent.MaybeEat(TokenType.Dot));

            if (qualifiers.Count > 1) {
                return new QualifiedNameExpression(parent, qualifiers);
            }

            return new SimpleNameExpression(parent, qualifiers[0]);
        }

        internal static NameExpression ParseName(SyntaxTreeNode parent, string name) {
            var qualifiers = new List<string>(name.Split('.'));
            if (qualifiers.Count > 1) {
                return new QualifiedNameExpression(parent, qualifiers);
            }

            return new SimpleNameExpression(parent, qualifiers[0]);
        }
    }
}