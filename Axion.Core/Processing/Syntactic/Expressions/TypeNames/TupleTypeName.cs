using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         tuple_type:
    ///             '(' [type {',' type}] ')';
    ///     </c>
    /// </summary>
    public class TupleTypeName : TypeName {
        private NodeList<TypeName> types;

        public NodeList<TypeName> Types {
            get => types;
            set => SetNode(ref types, value);
        }

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="TupleTypeName"/> from Axion tokens.
        /// </summary>
        public TupleTypeName(SyntaxTreeNode parent) {
            Parent = parent;
            Types  = new NodeList<TypeName>(this);

            MarkStart(TokenType.OpenParenthesis);
            if (!Peek.Is(TokenType.CloseParenthesis)) {
                do {
                    Types.Add(ParseTypeName(parent));
                } while (MaybeEat(TokenType.Comma));
            }

            Eat(TokenType.CloseParenthesis);
            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs new <see cref="TupleTypeName"/> from C# syntax.
        /// </summary>
        public TupleTypeName(SyntaxTreeNode parent, TupleTypeSyntax csNode) {
            Parent = parent;
            // TODO: add names for tuple items
            Types = new NodeList<TypeName>(
                this,
                csNode.Elements.Select(i => FromCSharp(this, i.Type))
            );
        }

        /// <summary>
        ///     Constructs plain <see cref="TupleTypeName"/> without position in source.
        /// </summary>
        public TupleTypeName(SyntaxTreeNode parent, IEnumerable<TypeName> types) {
            Parent = parent;
            Types  = new NodeList<TypeName>(this, types);
        }

        #endregion

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", types);
            c.Write(")");
        }

        #endregion
    }
}