using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;

// ReSharper disable once CheckNamespace
namespace Axion.Core.Specification {
    public partial class Spec {
        public partial class CSharp {
            public static readonly Dictionary<TokenType, string> BinaryOperators =
                new Dictionary<TokenType, string> {
                    { TokenType.OpAdd,                "+"  },
                    { TokenType.OpSubtract,           "-"  },
                    { TokenType.OpTrueDivide,         "/"  },
                    { TokenType.OpRemainder,          "%"  },
                    { TokenType.OpMultiply,           "*"  },
                
                    { TokenType.OpAnd,                "&&" },
                    { TokenType.OpOr,                 "||" },
                
                    { TokenType.OpBitwiseAnd,         "&"  },
                    { TokenType.OpBitwiseOr,          "|"  },
                    { TokenType.OpExclusiveOr,        "^"  },

                    { TokenType.OpEquals,             "==" },
                    { TokenType.OpNotEquals,          "!=" },
                
                    { TokenType.OpAs,                 "as" },
                    { TokenType.OpIs,                 "is" },

                
                    { TokenType.OpLessThan,           "<"  },
                    { TokenType.OpGreaterThan,        ">"  },
                    { TokenType.OpLessThanOrEqual,    "<=" },
                    { TokenType.OpGreaterThanOrEqual, ">=" }
                };
            
            public static readonly Dictionary<TokenType, string> CompoundAssignmentOperators =
                new Dictionary<TokenType, string> {
                    { TokenType.AddAssign,           "+="  },
                    { TokenType.TrueDivideAssign,    "/="  },
                    { TokenType.RemainderAssign,     "%="  },
                    { TokenType.MultiplyAssign,      "*="  },
                    { TokenType.SubtractAssign,      "-="  },
                    { TokenType.BitOrAssign,         "|="  },
                    { TokenType.BitAndAssign,        "&="  },
                    { TokenType.BitLeftShiftAssign,  "<<=" },
                    { TokenType.BitRightShiftAssign, ">>=" }
                };
        }
            
//            public static readonly Dictionary<TokenType, SyntaxKind> BinaryOperators =
//            new Dictionary<TokenType, SyntaxKind> {
//                { TokenType.OpAdd,                SyntaxKind.AddExpression },
//                { TokenType.OpSubtract,           SyntaxKind.SubtractExpression },
//                { TokenType.OpTrueDivide,         SyntaxKind.DivideExpression },
//                { TokenType.OpRemainder,          SyntaxKind.ModuloExpression },
//                { TokenType.OpMultiply,           SyntaxKind.MultiplyExpression },
//                
//                { TokenType.OpAnd,                SyntaxKind.LogicalAndExpression },
//                { TokenType.OpOr,                 SyntaxKind.LogicalOrExpression},
//                //{ TokenType.OpNot,                SyntaxKind.LogicalNotExpression },
//                
//                { TokenType.OpBitwiseAnd,         SyntaxKind.BitwiseAndExpression },
//                { TokenType.OpBitwiseOr,          SyntaxKind.BitwiseOrExpression },
//                { TokenType.OpExclusiveOr,        SyntaxKind.ExclusiveOrExpression },
//                //{ TokenType.OpBitwiseNot,         SyntaxKind.BitwiseNotExpression },
//
//                { TokenType.OpEquals,             SyntaxKind.EqualsExpression },
//                { TokenType.OpNotEquals,          SyntaxKind.NotEqualsExpression },
//                
//                { TokenType.OpAs,                 SyntaxKind.AsExpression },
//                { TokenType.OpIs,                 SyntaxKind.IsExpression },
//
//                
//                { TokenType.OpLessThan,           SyntaxKind.LessThanExpression },
//                { TokenType.OpGreaterThan,        SyntaxKind.GreaterThanExpression },
//                { TokenType.OpLessThanOrEqual,    SyntaxKind.LessThanOrEqualExpression },
//                { TokenType.OpGreaterThanOrEqual, SyntaxKind.GreaterThanOrEqualExpression }
//            };
//        
//        public static readonly Dictionary<TokenType, SyntaxKind> CompoundAssignOperators =
//            new Dictionary<TokenType, SyntaxKind> {
//                { TokenType.AddAssign,           SyntaxKind.AddAssignmentExpression },
//                { TokenType.TrueDivideAssign,    SyntaxKind.DivideAssignmentExpression },
//                { TokenType.RemainderAssign,     SyntaxKind.ModuloAssignmentExpression },
//                { TokenType.MultiplyAssign,      SyntaxKind.MultiplyAssignmentExpression },
//                { TokenType.SubtractAssign,      SyntaxKind.SubtractAssignmentExpression },
//                { TokenType.BitOrAssign,         SyntaxKind.OrAssignmentExpression },
//                { TokenType.BitAndAssign,        SyntaxKind.AndAssignmentExpression },
//                { TokenType.BitLeftShiftAssign,  SyntaxKind.LeftShiftAssignmentExpression },
//                { TokenType.BitRightShiftAssign, SyntaxKind.RightShiftAssignmentExpression }
//            };
//        }
    }
}