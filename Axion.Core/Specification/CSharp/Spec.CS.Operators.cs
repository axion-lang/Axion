using System.CodeDom;
using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Specification.CSharp {
    public class Spec {
        public static readonly Dictionary<TokenType, CodeBinaryOperatorType> CSharpBinaryOperators =
            new Dictionary<TokenType, CodeBinaryOperatorType> {
                { TokenType.OpAdd,                CodeBinaryOperatorType.Add },
                { TokenType.Assign,               CodeBinaryOperatorType.Assign },
                { TokenType.OpTrueDivide,         CodeBinaryOperatorType.Divide },
                { TokenType.OpRemainder,          CodeBinaryOperatorType.Modulus },
                { TokenType.OpMultiply,           CodeBinaryOperatorType.Multiply },
                { TokenType.OpSubtract,           CodeBinaryOperatorType.Subtract },
                { TokenType.OpLessThan,           CodeBinaryOperatorType.LessThan },
                { TokenType.OpBitwiseOr,          CodeBinaryOperatorType.BitwiseOr },
                { TokenType.KeywordOr,            CodeBinaryOperatorType.BooleanOr },
                { TokenType.OpBitwiseAnd,         CodeBinaryOperatorType.BitwiseAnd },
                { TokenType.KeywordAnd,           CodeBinaryOperatorType.BooleanAnd },
                { TokenType.OpGreaterThan,        CodeBinaryOperatorType.GreaterThan },
                //{ TokenType.OpEquals,             CodeBinaryOperatorType.ValueEquality },
                { TokenType.OpEquals,             CodeBinaryOperatorType.IdentityEquality },
                { TokenType.OpNotEquals,          CodeBinaryOperatorType.IdentityInequality },
                { TokenType.OpLessThanOrEqual,    CodeBinaryOperatorType.LessThanOrEqual },
                { TokenType.OpGreaterThanOrEqual, CodeBinaryOperatorType.GreaterThanOrEqual }
            };
    }
}