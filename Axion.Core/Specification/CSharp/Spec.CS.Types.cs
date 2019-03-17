using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable once CheckNamespace
namespace Axion.Core.Specification {
    public partial class Spec {
        public partial class CSharp {
            public static TypeSyntax MakeGenericListType(TypeSyntax constraint) {
                return SyntaxFactory.ParseTypeName("System.Generic.List<" + constraint + ">");
            }

            public static readonly Dictionary<TokenType, SyntaxKind> LiteralExpressions =
                new Dictionary<TokenType, SyntaxKind> {
                    { TokenType.Number, SyntaxKind.NumericLiteralExpression },
                    { TokenType.String, SyntaxKind.StringLiteralExpression },
                    { TokenType.Character, SyntaxKind.CharacterLiteralExpression },
                    { TokenType.KeywordTrue, SyntaxKind.TrueLiteralExpression },
                    { TokenType.KeywordFalse, SyntaxKind.FalseLiteralExpression }
                };
        }
    }
}