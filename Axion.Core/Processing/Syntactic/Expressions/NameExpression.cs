using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         name:
    ///             simple_name | qualified_name
    ///     </c>
    /// </summary>
    public abstract class NameExpression : Expression {
        public abstract string Name { get; }
        protected NameExpression() { }
        protected NameExpression(SyntaxTreeNode parent) : base(parent) { }

        internal static NameExpression ParseName(SyntaxTreeNode parent) {
            var qualifiers = new List<Token>();
            do {
                parent.Eat(Identifier);
                qualifiers.Add(parent.Token);
            } while (parent.MaybeEat(Dot));

            if (qualifiers.Count > 1) {
                return new QualifiedNameExpression(parent, qualifiers.Select(q => q.Value));
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